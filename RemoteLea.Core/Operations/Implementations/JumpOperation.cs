using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

[Operation("Jmp")]
[OperationParameter(LabelParam, ParameterType.String, "Label to jump to")]
public class JumpOperation : OperationBase
{
    private const string LabelParam = nameof(Arguments.Label);

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success(parsedArguments.Label));
    }
    
    private class Arguments
    {
        public string Label { get; set; } = null!;
    }
}