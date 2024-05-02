using System.Linq;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Operation that adds an integer to the specified variable. Supports negative numbers for
/// subtraction.
/// </summary>
[Operation(OpCode)]
[OperationParameter(0, ValueParam, ValueType, "Value to add")]
[OperationParameter(1, OutputParam, OutputType, "Variable to add the value to")]
public class AddOperation : OperationBase
{
    public const string OpCode = "add";
    public const string ValueParam = "Value";
    public const string OutputParam = "StorageVariable";
    private const ParameterType ValueType = ParameterType.Integer;
    private const ParameterType OutputType = ParameterType.VariableReference;

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var storageVariable = context.ParseVariableArgument(OutputParam);
        if (storageVariable == null)
        {
            context.LogInvalidRequiredArgument(OutputParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var intValue = context.ParseIntArgument(ValueParam);
        var variableValue = context.ParseVariableArgument(ValueParam);
        if (intValue == null && variableValue == null)
        {
            context.LogInvalidRequiredArgument(ValueParam, ParameterType.Integer | ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var numberToAdd = 0;
        if (intValue != null)
        {
            numberToAdd = intValue.Value;
        }
        else if (variableValue != null)
        {
            if (!context.Variables.TryGetValue(variableValue.Value.VariableName, out var valueVariable))
            {
                context.Log.ReferencedVariableDoesntExist(variableValue.Value.VariableName);
                return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
            }

            if (valueVariable is not int variableInt)
            {
                var message = $"Variable {variableValue.Value.VariableName} is a " +
                              $"{valueVariable.GetType().Name}, not an int";
                context.Log(LogLevel.Error, message);
                return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
            }

            numberToAdd = variableInt;
        }

        if (!context.Variables.TryGetValue(storageVariable.Value.VariableName, out var variable))
        {
            context.Log.ReferencedVariableDoesntExist(storageVariable.Value.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (variable is int integer)
        {
            var sum = integer + numberToAdd;
            context.Outputs[storageVariable.Value.VariableName] = sum;
            context.Log(LogLevel.Info, $"Added {numberToAdd} to {storageVariable.Value.VariableName} " +
                                       $"(new value = {sum})");
        }
        else
        {
            var message =
                $"Variable {storageVariable.Value.VariableName} is a {variable.GetType()}, not an int";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}