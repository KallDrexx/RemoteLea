using System;
using System.Collections.Generic;
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
    private readonly Dictionary<string, object> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object> _outputs = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _cancellationTokenSource = new();

    public ExecutionEngine(OperationManager operationManager, LogFunction logFunction)
    {
        _operationManager = operationManager;
        _logFunction = logFunction;
    }

    /// <summary>
    /// Executes the provided instruction set. Only one execution should be active at any given time.
    /// </summary>
    /// <param name="instructions"></param>
    public async Task Execute(InstructionSet instructions)
    {
        // TODO: Probably need a way to provide initial variables (e.g. an SPI bus that's already been created)
        // TODO: Add logging
        _cancellationTokenSource = new CancellationTokenSource();
        _variables.Clear();

        // We have to manage the instruction set enumerator ourselves to properly
        // move to labels.
        using var enumerator = instructions.GetEnumerator();
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            var instruction = enumerator.Current;
            if (instruction == null)
            {
                break;
            }

            var operation = _operationManager.Resolve(instruction.OpCode);
            if (operation == null)
            {
                // TODO: Better error handling
                var message = $"Unknown op code {instruction.OpCode}";
                throw new InvalidOperationException(message);
            }

            _outputs.Clear();
            var wasSuccessful = await operation.ExecuteAsync(
                instruction.Arguments, 
                _variables, 
                _outputs, 
                _logFunction,
                out var nextLabel);
            
            if (!wasSuccessful)
            {
                // TODO: log that we are stopping execution
                break;
            }

            // Any outputs mapped to a variable name in the instruction should be mapped
            foreach (var (outputKey, variableName) in instruction.OutputVariableNames)
            {
                if (!_outputs.TryGetValue(outputKey, out var outputValue))
                {
                    continue;
                }

                if (outputValue == null)
                {
                    _variables.Remove(outputKey);
                }
                else
                {
                    _variables[outputKey] = outputValue;
                }
            }

            if (nextLabel != null)
            {
                enumerator.MoveToLabel(nextLabel);
            }
            else
            {
                enumerator.MoveNext();
            }
        }

        // TODO: We should have some way to track things that need to be "disposed". E.g. if an instruction
        // set creates a digital input, or adds an event to an interrupt, we should probably have some way
        // to remove it after the run so the instruction set can idempotently executed.
    }
}