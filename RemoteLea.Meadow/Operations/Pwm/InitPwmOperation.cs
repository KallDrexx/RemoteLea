using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Pwm;

/// <summary>
/// Creates a PWM port for the provided pin and saves it into the specified variable. PWm is started
/// with a zero duty cycle.
/// </summary>
[Operation("init_pwm")]
[OperationParameter(0, PinNameParam, ParameterType.String, "Name of the pin to create a PWM port for.")]
[OperationParameter(1, FrequencyParam, ParameterType.Integer, "Frequency in hz")]
[OperationParameter(2, VariableParam, ParameterType.VariableReference, "Variable to store the pwm port with.")]
public class InitPwmOperation : OperationBase
{
    private const string PinNameParam = "PinName";
    private const string VariableParam = "Variable";
    private const string FrequencyParam = "Frequency";

    private readonly IPwmOutputController _pwmOutputController;
    private readonly PinLookup _pins;

    public InitPwmOperation(IPwmOutputController pwmOutputController, PinLookup pinLookup)
    {
        _pwmOutputController = pwmOutputController ?? throw new ArgumentNullException(nameof(pwmOutputController));
        _pins = pinLookup ?? throw new ArgumentNullException(nameof(pinLookup));
    }

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var pinName = context.ParseStringArgument(PinNameParam);
        if (pinName == null)
        {
            context.LogInvalidRequiredArgument(PinNameParam, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var frequency = context.ParseIntArgument(FrequencyParam);
        if (frequency == null)
        {
            context.LogInvalidRequiredArgument(FrequencyParam, ParameterType.Integer);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var variable = context.ParseVariableArgument(VariableParam);
        if (variable == null)
        {
            context.LogInvalidRequiredArgument(VariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var pin = _pins.Get(pinName);
        if (pin == null)
        {
            var message = $"No known pin with the name '{pinName}'";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var pwmPort = _pwmOutputController.CreatePwmPort(pin,
            new Frequency(frequency.Value, Frequency.UnitType.Hertz), 0f);

        pwmPort.Start();

        context.Outputs.Add(variable.Value.VariableName, pwmPort);

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}