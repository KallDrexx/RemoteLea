using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Operation that adds an integer to the specified variable. Supports negative numbers for
/// subtraction.
/// </summary>
[Operation(OpCode)]
[OperationParameter(ValueParam, ValueType, "Value to add")]
[OperationParameter(OutputParam, OutputType, "Variable to add the value to")]
public class AddOperation : OperationBase
{
    public const string OpCode = "add";
    public const string ValueParam = "Value";
    public const string OutputParam = nameof(Arguments.StorageVariable);
    private const ParameterType ValueType = ParameterType.Integer;
    private const ParameterType OutputType = ParameterType.VariableReference;

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        int numberToAdd = 0;
        if (parsedArguments.IntValue != null)
        {
            numberToAdd = parsedArguments.IntValue.Value;
        }
        else if (parsedArguments.ValueVariable != null)
        {
            if (!context.Variables.TryGetValue(parsedArguments.ValueVariable.Value.VariableName, out var valueVariable))
            {
                context.Log.ReferencedVariableDoesntExist(parsedArguments.ValueVariable.Value.VariableName);
                return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
            }

            if (valueVariable is not int intValue)
            {
                var message = $"Variable {parsedArguments.ValueVariable.Value.VariableName} is a " +
                              $"{valueVariable.GetType().Name}, not an int";
                context.Log(LogLevel.Error, message);
                return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
            }

            numberToAdd = intValue;
        }

        if (!context.Variables.TryGetValue(parsedArguments.StorageVariable.VariableName, out var variable))
        {
            context.Log.ReferencedVariableDoesntExist(parsedArguments.StorageVariable.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (variable is int integer)
        {
            var sum = integer + numberToAdd;
            context.Outputs[parsedArguments.StorageVariable.VariableName] = sum;
            context.Log(LogLevel.Info, $"Added {numberToAdd} to {parsedArguments.StorageVariable.VariableName} " +
                                       $"(new value = {sum})");
        }
        else
        {
            var message =
                $"Variable {parsedArguments.StorageVariable.VariableName} is a {variable.GetType()}, not an int";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }

    private class Arguments
    {
        [ExecutionArgument(nameOverride: ValueParam, isRequired: false)]
        public int? IntValue { get; set; }

        [ExecutionArgument(nameOverride: ValueParam, isRequired: false)]
        public VariableReferenceArgumentValue? ValueVariable { get; set; }

        public VariableReferenceArgumentValue StorageVariable { get; set; }
    }
}