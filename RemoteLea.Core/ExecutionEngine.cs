﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RemoteLea.Core.Operations;

namespace RemoteLea.Core;

/// <summary>
/// Manages the execution of instruction sets.
/// </summary>
public class ExecutionEngine
{
    private readonly OperationManager _operationManager;
    private readonly LogFunction _logFunction;
    protected readonly Dictionary<string, object> Variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object> _outputs = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _cancellationTokenSource = new();
    private Task? _currentExecution;

    public ExecutionEngine(OperationManager operationManager, LogFunction logFunction)
    {
        _operationManager = operationManager;
        _logFunction = logFunction;
    }

    /// <summary>
    /// Executes the provided instruction set. Only one execution should be active at any given time.
    /// </summary>
    public async Task Execute(InstructionSet instructions)
    {
        try
        {
            var currentTask = _currentExecution;
            if (currentTask != null)
            {
                _cancellationTokenSource.Cancel();
                var timeoutTask = Task.Run(async () => await Task.Delay(1000));
                await Task.WhenAny(currentTask, timeoutTask);
            }

            _currentExecution = Task.Run(async () => await ExecuteInternal(instructions));
        }
        catch (Exception exception)
        {
            _logFunction(LogLevel.Error, 0, GetType().Name, $"Execution exception: {exception}");
        }
    }
    
    private async Task ExecuteInternal(InstructionSet instructions)
    {
        try
        {
            ClearVariables();

            _cancellationTokenSource = new CancellationTokenSource();
            var executionContext = new OperationExecutionContext(Variables, _outputs, _cancellationTokenSource.Token);

            // We have to manage the instruction set enumerator ourselves to properly
            // move to labels.
            using var enumerator = instructions.GetEnumerator();
            enumerator.MoveNext();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // In case no true asynchronous code has been called, allow other async tasks to execute.
                // This helps prevent infinite loops that can't ever be cancelled.
                await Task.Yield();

                var instruction = enumerator.Current;
                if (instruction == null)
                {
                    break;
                }

                var instructionIndex = enumerator.CurrentIndex;
                _logFunction(LogLevel.Debug, instructionIndex, GetType().Name,
                    $"Executing instruction {instruction.OpCode}");

                var operation = _operationManager.Resolve(instruction.OpCode);
                if (operation == null)
                {
                    // TODO: Better error handling
                    var message = $"Unknown op code '{instruction.OpCode}'";
                    throw new InvalidOperationException(message);
                }

                _outputs.Clear();

                void LogFunction(LogLevel level, string message)
                {
                    _logFunction(level, instructionIndex, operation.GetType().Name, message);
                }

                executionContext.Arguments = instruction.Arguments;
                executionContext.Log = LogFunction;
                var result = await operation.ExecuteAsync(executionContext);

                if (!result.WasSuccessful)
                {
                    _logFunction(LogLevel.Error, instructionIndex, GetType().Name, "Instruction failed, stopping");
                    break;
                }

                // Any outputs should be mapped to their variable names
                foreach (var (variableName, value) in executionContext.Outputs)
                {
                    if (value == null)
                    {
                        Variables.Remove(variableName);
                    }
                    else
                    {
                        Variables[variableName] = value;
                    }
                }

                if (result.JumpToLabel != null)
                {
                    enumerator.MoveToLabel(result.JumpToLabel);
                }
                else
                {
                    enumerator.MoveNext();
                }
            }

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                _logFunction(LogLevel.Info, enumerator.CurrentIndex, GetType().Name, "Execution cancelled");
            }
            else
            {
                _logFunction(LogLevel.Info, 0, GetType().Name, "Execution finished");
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore task cancellation
        }
        catch (Exception exception)
        {
            _logFunction(LogLevel.Error, 0, GetType().Name, $"Execution exception: {exception}");
        }
    }

    /// <summary>
    /// Cancels an execution that's in progress
    /// </summary>
    public void CancelCurrentExecution()
    {
        _cancellationTokenSource.Cancel();
    }

    private void ClearVariables()
    {
        // Dispose any disposable variables
        foreach (var (key, value) in Variables)
        {
            if (value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        Variables.Clear();
    }


    private class OperationExecutionContext : IOperationExecutionContext
    {
        public IReadOnlyDictionary<string, IArgumentValue> Arguments { get; set; } = null!;
        public InstructionLogFunction Log { get; set; } = null!;
        public IReadOnlyDictionary<string, object> Variables { get; }
        public Dictionary<string, object> Outputs { get; }
        public CancellationToken CancellationToken { get; }

        public OperationExecutionContext(
            IReadOnlyDictionary<string, object> variables,
            Dictionary<string, object> outputs,
            CancellationToken cancellationToken)
        {
            Variables = variables;
            Outputs = outputs;
            CancellationToken = cancellationToken;
        }
    }
}