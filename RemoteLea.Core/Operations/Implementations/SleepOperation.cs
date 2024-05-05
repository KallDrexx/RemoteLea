using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

[Operation(OpCode)]
[OperationParameter(0, TimeParam, ParameterType.Integer, "Number of milliseconds to wait for")]
public class SleepOperation : OperationBase
{
    public const string OpCode = "sleep";
    public const string TimeParam = "Milliseconds";

    protected override async ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var ms = context.ParseIntArgument(TimeParam);
        if (ms == null)
        {
            context.LogInvalidRequiredArgument(TimeParam, ParameterType.Integer);
            return OperationExecutionResult.Failure();
        }
        
        await Task.Delay(ms.Value, context.CancellationToken);
        return OperationExecutionResult.Success();
    }
}