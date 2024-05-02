using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

[Operation(OpCode)]
[OperationParameter(0, LabelParam, ParameterType.String, "Label to jump to")]
public class JumpOperation : OperationBase
{
    public const string OpCode = "jmp";
    public const string LabelParam = "Label";

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var label = context.ParseStringArgument(LabelParam);
        if (label == null)
        {
            context.LogInvalidRequiredArgument(LabelParam, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success(label));
    }
}