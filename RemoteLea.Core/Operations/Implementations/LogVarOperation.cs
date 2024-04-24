using System;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

[Operation(OpCode)]
[OperationParameter(0, VariableParam, ParameterType.VariableReference, "Variable to log the value of")]
public class LogVarOperation : OperationBase
{
    public const string OpCode = "logvar";
    public const string VariableParam = nameof(Arguments.Variable);
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(parsedArguments.Variable, out var variable))
        {
            context.Log.ReferencedVariableDoesntExist(parsedArguments.Variable);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var stringValue = variable switch
        {
            byte[] array => BitConverter.ToString(array),
            _ => variable?.ToString(),
        };

        var message = $"Variable {parsedArguments.Variable} has the value of '{stringValue}'";
        context.Log(LogLevel.Info, message);

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }

    private class Arguments
    {
        public string Variable { get; set; } = null!;
    }
}