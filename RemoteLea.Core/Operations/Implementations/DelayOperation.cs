using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

[Operation(OpCode)]
[OperationParameter(TimeParam, ParameterType.Integer, "Number of milliseconds to wait for")]
public class DelayOperation : OperationBase
{
    public const string OpCode = "dly";
    
    private const string TimeParam = nameof(Arguments.Milliseconds);

    protected override async ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return OperationExecutionResult.Failure();
        }

        await Task.Delay(parsedArguments.Milliseconds, context.CancellationToken);
        return OperationExecutionResult.Success();
    }
    
    private class Arguments
    {
        public int Milliseconds { get; set; }
    }
}