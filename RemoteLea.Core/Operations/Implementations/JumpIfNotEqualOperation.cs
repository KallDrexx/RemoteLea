using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations.Implementations;

/// <summary>
/// Jumps to the specified label if two variables are *not* equal. If both variables are equal than the
/// next instruction is executed. An error occurs if either variable referenced does not exist. If both
/// variables exist but are of different types, then they are counted as not equal and a warning is
/// logged.
/// </summary>
public class JumpIfNotEqualOperation : OperationBase
{
    public const string OpCodeValue = "jne";
    
    public const string FirstVar = "First";
    public const string SecondVar = "Second";
    public const string LabelParam = "Label";
    
    protected override string OpCode => OpCodeValue;
    protected override IReadOnlyList<OperationParameter> Parameters => new []
    {
        new OperationParameter(FirstVar, ParameterType.VariableReference, "First variable for comparison"),
        new OperationParameter(SecondVar, ParameterType.VariableReference, "Second variable to compare"),
        new OperationParameter(LabelParam, ParameterType.String, "Label to jump to if variables are equal")
    };
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var firstVar = context.ParseVariableArgument(FirstVar);
        var secondVar = context.ParseVariableArgument(SecondVar);
        var label = context.ParseStringArgument(LabelParam);

        if (firstVar == null)
        {
            context.LogInvalidRequiredArgument(FirstVar, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (secondVar == null)
        {
            context.LogInvalidRequiredArgument(SecondVar, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (label == null)
        {
            context.LogInvalidRequiredArgument(LabelParam, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(firstVar.Value.VariableName, out var first))
        {
            context.Log.ReferencedVariableDoesntExist(firstVar.Value.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(secondVar.Value.VariableName, out var second))
        {
            context.Log.ReferencedVariableDoesntExist(secondVar.Value.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (first.GetType() != second.GetType())
        {
            var message = $"Variable `{firstVar.Value.VariableName}` has a type of {first.GetType().Name} while " +
                          $"variable `{secondVar.Value.VariableName}` has a type of {second.GetType().Name}. Considering" +
                          $"them as not equal";

            context.Log(LogLevel.Warning, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success(label));
        }

        return first.Equals(second)
            ? new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success())
            : new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success(label));
    }
}