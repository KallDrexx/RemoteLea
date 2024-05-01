using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        Resolver.CommandService.Subscribe((command) =>
        {
            if (!command.Arguments.TryGetValue("input", out var input) || input is not string strInput)
            {
                Resolver.Log.Info("Received command with no input value");
                return;
            }

            Resolver.Log.Info("New instruction set received");
            InstructionSet instructions;
            try
            {
                instructions = serializer.Deserialize(strInput);
            }
            catch (InstructionDeserializationException exception)
            {
                var message = $"Failed to parse instruction set on line {exception.Line} " +
                              $"character {exception.CharIndex}: {exception.Message}";
                
                Resolver.Log.Error(message);
                return;
            }
            
            executionEngine.CancelCurrentExecution();
            
            Resolver.Log.Info("Executing instruction set");
            Task.Run(async () => await executionEngine.Execute(instructions));
        });
        
        Resolver.Log.Info("Starting RemoteLea Meadow Execution Engine...");
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
            
            case LogLevel.Debug:
                Resolver.Log.Debug(finalMessage);
                break;
            
            default:
                Resolver.Log.Warn($"RemoteLea log level of {level} not supported");
                break;
        }
    }
}