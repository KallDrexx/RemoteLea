using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

public class JumpOperation : OperationBase
{
    public const string OpCodeValue = "jump";
    public const string LabelParam = "Label";
    
    protected override string OpCode => OpCodeValue;
    protected override IReadOnlyList<OperationParameter> Parameters => new []
    {
        new OperationParameter(LabelParam, ParameterType.String, "Label to jump to")
    };

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