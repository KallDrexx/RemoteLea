using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Jumps to the specified label if two variables are *not* equal. If both variables are equal than the
/// next instruction is executed. An error occurs if either variable referenced does not exist. If both
/// variables exist but are of different types, then they are counted as not equal and a warning is
/// logged.
/// </summary>
[Operation(OpCode)]
[OperationParameter(0, FirstVar, ParameterType.VariableReference, "First variable for comparison")]
[OperationParameter(1, SecondVar, ParameterType.VariableReference, "Second variable to compare")]
[OperationParameter(2, LabelParam, ParameterType.String, "Label to jump to if variables are equal")]
public class JumpIfNotEqualOperation : OperationBase
{
    public const string OpCode = "jne";
    
    public const string FirstVar = nameof(Arguments.First);
    public const string SecondVar = nameof(Arguments.Second);
    public const string LabelParam = nameof(Arguments.Label);
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArgs = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArgs == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(parsedArgs.First.VariableName, out var first))
        {
            context.Log.ReferencedVariableDoesntExist(parsedArgs.First.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(parsedArgs.Second.VariableName, out var second))
        {
            context.Log.ReferencedVariableDoesntExist(parsedArgs.Second.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (first.GetType() != second.GetType())
        {
            var message = $"Variable `{parsedArgs.First.VariableName}` has a type of {first.GetType().Name} while " +
                          $"variable `{parsedArgs.Second.VariableName}` has a type of {second.GetType().Name}. Considering" +
                          $"them as not equal";

            context.Log(LogLevel.Warning, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success(parsedArgs.Label));
        }

        return first.Equals(second)
            ? new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success())
            : new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success(parsedArgs.Label));
    }

    private class Arguments
    {
        public VariableReferenceArgumentValue First { get; set; }
        public VariableReferenceArgumentValue Second { get; set; }
        public string Label { get; set; } = null!;
    }
}