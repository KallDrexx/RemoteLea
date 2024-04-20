using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Operation that adds an integer to the specified variable. Supports negative numbers for
/// subtraction.
/// </summary>
[Operation("Add")]
[OperationParameter(ValueParam, ValueType, "Value to add")]
[OperationParameter(OutputParam, OutputType, "Variable to add the value to")]
public class AddOperation : OperationBase
{
    private const string ValueParam = nameof(Arguments.Value);
    private const string OutputParam = nameof(Arguments.Variable);
    private const ParameterType ValueType = ParameterType.Integer;
    private const ParameterType OutputType = ParameterType.VariableReference;

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(parsedArguments.Variable.VariableName, out var variable))
        {
            var message = $"Variable `{parsedArguments.Variable.VariableName}` does not exist";
            context.Log(LogLevel.Error, nameof(AddOperation), message);

            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (variable is int integer)
        {
            var sum = integer + parsedArguments.Value;
            context.Outputs[parsedArguments.Variable.VariableName] = sum;
            context.Log(LogLevel.Info,
                nameof(AddOperation),
                $"Added {parsedArguments.Value} to {parsedArguments.Variable.VariableName} (new value = {sum})");
        }
        else
        {
            var message = $"Variable {parsedArguments.Variable.VariableName} is a {variable.GetType()}, not an int";
            context.Log(LogLevel.Error, nameof(AddOperation), message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }

    private class Arguments
    {
        public int Value { get; set; }
        public VariableReferenceArgumentValue Variable { get; set; }
    }
}