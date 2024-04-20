using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations;

/// <summary>
/// Operation that allows setting a variable to a specified value
/// </summary>
[Operation("SetVal")]
[OperationParameter(ValueParam, ValueTypes, "Value to set the variable to")]
[OperationParameter(VariableOutput, OutputTypes, "Variable to set the value for")]
public class SetValueOperation : OperationBase
{
    private const string ValueParam = "Value";
    private const string VariableOutput = "Variable";
    private const ParameterType ValueTypes = ParameterType.Any;
    private const ParameterType OutputTypes = ParameterType.VariableReference;

    public override ValueTask<bool> ExecuteAsync(
        IReadOnlyDictionary<string, IArgumentValue> arguments,
        IReadOnlyDictionary<string, object> variables,
        Dictionary<string, object> outputs,
        LogFunction log,
        out string? jumpToAddress)
    {
        jumpToAddress = null;

        var parsedArguments = FillFromArguments<Arguments>(arguments, log);
        if (parsedArguments == null)
        {
            return new ValueTask<bool>(false);
        }

        if (parsedArguments.IntValue != null)
        {
            outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.IntValue.Value;
            log(LogLevel.Info, nameof(SetValueOperation),
                $"Set {parsedArguments.OutputVariable.VariableName} to {parsedArguments.IntValue}");
        }
        else if (parsedArguments.BoolValue != null)
        {
            outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.BoolValue.Value;
            log(LogLevel.Info, nameof(SetValueOperation),
                $"Set {parsedArguments.OutputVariable.VariableName} to {parsedArguments.BoolValue}");
        }
        else if (parsedArguments.StringValue != null)
        {
            outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.StringValue;
            log(LogLevel.Info, nameof(SetValueOperation),
                $"Set {parsedArguments.OutputVariable.VariableName} to {parsedArguments.StringValue}");
        }
        else if (parsedArguments.ByteArrayValue != null)
        {
            outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.ByteArrayValue;
            log(LogLevel.Info,
                nameof(SetValueOperation),
                $"Set {parsedArguments.OutputVariable.VariableName} to {Convert.ToString(parsedArguments.ByteArrayValue)}");
        }
        else if (parsedArguments.VariableValue != null)
        {
            if (!variables.TryGetValue(parsedArguments.VariableValue.Value.VariableName, out var variable))
            {
                log(LogLevel.Error,
                    nameof(SetValueOperation),
                    $"Referenced variable {parsedArguments.VariableValue.Value.VariableName}` does not exist");
                return new ValueTask<bool>(false);
            }

            outputs[parsedArguments.OutputVariable.VariableName] = variable;
            log(LogLevel.Info, nameof(SetValueOperation),
                $"Set {parsedArguments.OutputVariable.VariableName} to {variable}");
        }
        else
        {
            // TODO: Log if it was provided but an unknown type
            log.RequiredArgumentNotProvided(ValueParam, nameof(SetValueOperation));
            return new ValueTask<bool>(false);
        }

        return new ValueTask<bool>(true);
    }

    private class Arguments
    {
        [ExecutionArgument(nameOverride: ValueParam, isRequired: false)]
        public int? IntValue { get; set; }

        [ExecutionArgument(nameOverride: ValueParam, isRequired: false)]
        public string? StringValue { get; set; }

        [ExecutionArgument(nameOverride: ValueParam, isRequired: false)]
        public bool? BoolValue { get; set; }

        [ExecutionArgument(nameOverride: ValueParam, isRequired: false)]
        public byte[]? ByteArrayValue { get; set; }

        [ExecutionArgument(nameOverride: ValueParam, isRequired: false)]
        public VariableReferenceArgumentValue? VariableValue { get; set; }

        [ExecutionArgument(nameOverride: VariableOutput, isRequired: true)]
        public VariableReferenceArgumentValue OutputVariable { get; set; }
    }
}