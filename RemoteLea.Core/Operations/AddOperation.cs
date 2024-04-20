using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations;

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

        if (!variables.TryGetValue(parsedArguments.Variable.VariableName, out var variable))
        {
            var message = $"Variable `{parsedArguments.Variable.VariableName}` does not exist";
            log(LogLevel.Error, nameof(AddOperation), message);

            return new ValueTask<bool>(false);
        }

        if (variable is int integer)
        {
            var sum = integer + parsedArguments.Value;
            outputs[parsedArguments.Variable.VariableName] = sum;
            log(LogLevel.Info,
                nameof(AddOperation),
                $"Added {parsedArguments.Value} to {parsedArguments.Variable.VariableName} (new value = {sum})");
        }
        else
        {
            var message = $"Variable {parsedArguments.Variable.VariableName} is a {variable.GetType()}, not an int";
            log(LogLevel.Error, nameof(AddOperation), message);
        }

        return new ValueTask<bool>(true);
    }

    private class Arguments
    {
        public int Value { get; set; }
        public VariableReferenceArgumentValue Variable { get; set; }
    }
}