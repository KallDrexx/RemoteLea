using System;
using System.Collections.Generic;
using Meadow;
using RemoteLea.Core;
using RemoteLea.Core.Operations;
using RemoteLea.Serialization.AsmStyle;

namespace RemoteLea.Meadow;

public class MeadowLeaRunner
{
    public MeadowLeaRunner(IEnumerable<OperationBase> operations)
    {
        var manager = new OperationManager();
        foreach (var operation in operations)
        {
            manager.Register(operation);
        }
        
        var executionEngine = new ExecutionEngine(manager, LogExecution);
        var serializer = new InstructionSerializer(manager);
        Resolver.CommandService.Subscribe<AsmInstructionExecutionCommand>(command =>
        {
            if (string.IsNullOrWhiteSpace(command.Input))
            {
                return;
            }

            Resolver.Log.Info("New instruction set received");
            InstructionSet instructions;
            try
            {
                instructions = serializer.Deserialize(command.Input);
            }
            catch (InstructionDeserializationException exception)
            {
                var message = $"Failed to parse instruction set on line {exception.Line} " +
                              $"character {exception.CharIndex}: {exception.Message}";
                
                Resolver.Log.Error(message);
                return;
            }
            
            executionEngine.CancelCurrentExecution();
            _ = executionEngine.Execute(instructions);
        });
        
        Resolver.Log.Info("Starting RemoteLae Meadow Runner");
    }

    private void LogExecution(LogLevel level, int instructionIndex, string operationName, string message)
    {
        var finalMessage = $"{operationName} (#{instructionIndex}) {level}: {message}";
        switch (level)
        {
            case LogLevel.Info:
                Resolver.Log.Info(finalMessage);
                break;
            
            case LogLevel.Warning:
                Resolver.Log.Warn(finalMessage);
                break;
            
            case LogLevel.Error:
                Resolver.Log.Error(finalMessage);
                break;
            
            default:
                throw new NotSupportedException(level.ToString());
        }
    }
}