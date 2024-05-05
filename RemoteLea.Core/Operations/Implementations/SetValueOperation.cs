using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Operation that allows setting a variable to a specified value
/// </summary>
public class SetValueOperation : OperationBase
{
    public const string OpCodeValue = "set";
    public const string ValueParam = "Value";
    public const string VariableOutput = "Variable";
    private const ParameterType ValueTypes = ParameterType.Any;
    private const ParameterType OutputTypes = ParameterType.VariableReference;
    
    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(VariableOutput, OutputTypes, "Variable to set the value for"),
        new OperationParameter(ValueParam, ValueTypes, "Value to set the variable to")
    };

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var intValue = context.ParseIntArgument(ValueParam);
        var stringValue = context.ParseStringArgument(ValueParam);
        var boolValue = context.ParseBoolArgument(ValueParam);
        var byteArrayValue = context.ParseByteArrayArgument(ValueParam);
        var variableValue = context.ParseVariableArgument(ValueParam);

        var outputVariable = context.ParseVariableArgument(VariableOutput);
        if (outputVariable == null)
        {
            context.LogInvalidRequiredArgument(VariableOutput, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (intValue != null)
        {
            context.Outputs[outputVariable.Value.VariableName] = intValue.Value;
            context.Log(LogLevel.Debug, $"Set {outputVariable.Value.VariableName} to {intValue}");
        }
        else if (boolValue != null)
        {
            context.Outputs[outputVariable.Value.VariableName] = boolValue.Value;
            context.Log(LogLevel.Info, $"Set {outputVariable.Value.VariableName} to {boolValue}");
        }
        else if (stringValue != null)
        {
            context.Outputs[outputVariable.Value.VariableName] = stringValue;
            context.Log(LogLevel.Info, $"Set {outputVariable.Value.VariableName} to {stringValue}");
        }
        else if (byteArrayValue != null)
        {
            context.Outputs[outputVariable.Value.VariableName] = byteArrayValue;
            context.Log(LogLevel.Info,
                $"Set {outputVariable.Value.VariableName} to {Convert.ToString(byteArrayValue)}");
        }
        else if (variableValue != null)
        {
            if (!context.Variables.TryGetValue(variableValue.Value.VariableName, out var variable))
            {
                context.Log.ReferencedVariableDoesntExist(variableValue.Value.VariableName);
                return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
            }

            context.Outputs[outputVariable.Value.VariableName] = variable;
            context.Log(LogLevel.Info, $"Set {outputVariable.Value.VariableName} to {variable}");
        }
        else
        {
            context.LogInvalidRequiredArgument(ValueParam, ValueTypes);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}