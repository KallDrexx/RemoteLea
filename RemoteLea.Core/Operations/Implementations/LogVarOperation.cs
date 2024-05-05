using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

public class LogVarOperation : OperationBase
{
    public const string OpCodeValue = "log_var";
    public const string VariableParam = "Variable";
    
    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(VariableParam, ParameterType.VariableReference, "Variable to log the value of"),
    };

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var variable = context.ParseVariableArgument(VariableParam);
        if (variable == null)
        {
            context.LogInvalidRequiredArgument(VariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (!context.Variables.TryGetValue(variable.Value.VariableName, out var variableValue))
        {
            context.Log.ReferencedVariableDoesntExist(variable.Value.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var stringValue = variableValue switch
        {
            byte[] array => BitConverter.ToString(array),
            _ => variable.ToString(),
        };

        var message = $"Variable {variable.Value.VariableName} has the value of '{stringValue}'";
        context.Log(LogLevel.Info, message);

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}