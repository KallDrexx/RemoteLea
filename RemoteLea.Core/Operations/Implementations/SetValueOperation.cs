using System;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Operation that allows setting a variable to a specified value
/// </summary>
[Operation(OpCode)]
[OperationParameter(0, VariableOutput, OutputTypes, "Variable to set the value for")]
[OperationParameter(1, ValueParam, ValueTypes, "Value to set the variable to")]
public class SetValueOperation : OperationBase
{
    public const string OpCode = "set";
    public const string ValueParam = "Value";
    public const string VariableOutput = "Variable";
    private const ParameterType ValueTypes = ParameterType.Any;
    private const ParameterType OutputTypes = ParameterType.VariableReference;

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (parsedArguments.IntValue != null)
        {
            context.Outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.IntValue.Value;
            context.Log(LogLevel.Info,
                $"Set {parsedArguments.OutputVariable.VariableName} to {parsedArguments.IntValue}");
        }
        else if (parsedArguments.BoolValue != null)
        {
            context.Outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.BoolValue.Value;
            context.Log(LogLevel.Info,
                $"Set {parsedArguments.OutputVariable.VariableName} to {parsedArguments.BoolValue}");
        }
        else if (parsedArguments.StringValue != null)
        {
            context.Outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.StringValue;
            context.Log(LogLevel.Info,
                $"Set {parsedArguments.OutputVariable.VariableName} to {parsedArguments.StringValue}");
        }
        else if (parsedArguments.ByteArrayValue != null)
        {
            context.Outputs[parsedArguments.OutputVariable.VariableName] = parsedArguments.ByteArrayValue;
            context.Log(LogLevel.Info,
                $"Set {parsedArguments.OutputVariable.VariableName} to {Convert.ToString(parsedArguments.ByteArrayValue)}");
        }
        else if (parsedArguments.VariableValue != null)
        {
            if (!context.Variables.TryGetValue(parsedArguments.VariableValue.Value.VariableName, out var variable))
            {
                context.Log.ReferencedVariableDoesntExist(parsedArguments.VariableValue.Value.VariableName);
                return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
            }

            context.Outputs[parsedArguments.OutputVariable.VariableName] = variable;
            context.Log(LogLevel.Info, $"Set {parsedArguments.OutputVariable.VariableName} to {variable}");
        }
        else
        {
            // TODO: Log if it was provided but an unknown type
            context.Log.RequiredArgumentNotProvided(ValueParam);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
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