using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Jumps to the specified label if two variables are equal. If both variables are not equal than the
/// next instruction is executed. An error occurs if either variable referenced does not exist. If both
/// variables exist but are of different types, then they are counted as not equal and a warning is
/// logged.
/// </summary>
[Operation("jeq")]
[OperationParameter(FirstVar, ParameterType.VariableReference, "First variable for comparison")]
[OperationParameter(SecondVar, ParameterType.VariableReference, "Second variable to compare")]
[OperationParameter(LabelParam, ParameterType.String, "Label to jump to if variables are equal")]
public class JumpIfEqualOperation : OperationBase
{
    private const string FirstVar = nameof(Arguments.First);
    private const string SecondVar = nameof(Arguments.Second);
    private const string LabelParam = nameof(Arguments.Label);
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArgs = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArgs == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(parsedArgs.First.VariableName, out var first))
        {
            context.Log.ReferencedVariableDoesntExist(GetType().Name, parsedArgs.First.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(parsedArgs.Second.VariableName, out var second))
        {
            context.Log.ReferencedVariableDoesntExist(GetType().Name, parsedArgs.Second.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (first.GetType() != second.GetType())
        {
            var message = $"Variable `{parsedArgs.First.VariableName}` has a type of {first.GetType().Name} while " +
                          $"variable `{parsedArgs.Second.VariableName}` has a type of {second.GetType().Name}. Considering" +
                          $"them as not equal";

            context.Log(LogLevel.Warning, GetType().Name, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
        }

        return first == second 
            ? new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success(parsedArgs.Label)) 
            : new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }

    private class Arguments
    {
        public VariableReferenceArgumentValue First { get; set; }
        public VariableReferenceArgumentValue Second { get; set; }
        public string Label { get; set; } = null!;
    }
}