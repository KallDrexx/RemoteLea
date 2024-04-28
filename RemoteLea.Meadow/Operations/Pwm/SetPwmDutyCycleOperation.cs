using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Pwm;

[Operation("pwmduty")]
[OperationParameter(0, VariableParam, ParameterType.VariableReference, "Variable with the pwm port")]
[OperationParameter(1, DutyCycleParam, ParameterType.Integer,
    "Percent between 0 and 100 for the duty cycle of the pwm")]
public class SetPwmDutyCycleOperation : OperationBase
{
    private const string VariableParam = nameof(Arguments.Variable);
    private const string DutyCycleParam = nameof(Arguments.DutyCyclePercent);

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(parsedArguments.Variable.VariableName, out var variable))
        {
            var message = $"No known variable with the name '{parsedArguments.Variable.VariableName}'";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (variable is not IPwmPort pwmPort)
        {
            var message = $"Variable '{parsedArguments.Variable.VariableName}' is a {variable.GetType().Name} " +
                          $"but an IPwmPort was expected.";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (parsedArguments.DutyCyclePercent is < 0 or > 100)
        {
            var message = $"Duty cycle percent should be between 0 and 100, " +
                          $"but instead was {parsedArguments.DutyCyclePercent}";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        pwmPort.DutyCycle = parsedArguments.DutyCyclePercent / 100f;
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }

    private class Arguments
    {
        public VariableReferenceArgumentValue Variable { get; set; }
        public float DutyCyclePercent { get; set; }
    }
}