using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

public class SleepOperation : OperationBase
{
    public const string OpCodeValue = "sleep";
    public const string TimeParam = "Milliseconds";
    
    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(TimeParam, ParameterType.Integer, "Number of milliseconds to wait for"),
    };

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