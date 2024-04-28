using System;
using System.Collections.Generic;
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
[Operation("initpwm")]
[OperationParameter(0, PinNameParam, ParameterType.String, "Name of the pin to create a PWM port for.")]
[OperationParameter(1, FrequencyParam, ParameterType.Integer, "Frequency in hz")]
[OperationParameter(2, VariableParam, ParameterType.VariableReference, "Variable to store the pwm port with.")]
public class InitPwmOperation : OperationBase
{
    private const string PinNameParam = nameof(Arguments.PinName);
    private const string VariableParam = nameof(Arguments.Variable);
    private const string FrequencyParam = nameof(Arguments.Frequency);

    private readonly IPwmOutputController _pwmOutputController;
    private readonly Dictionary<string, IPin> _pins = new(StringComparer.OrdinalIgnoreCase);

    public InitPwmOperation(IPinDefinitions pinDefinitions, IPwmOutputController pwmOutputController)
    {
        if (pinDefinitions == null) throw new ArgumentNullException(nameof(pinDefinitions));

        _pwmOutputController = pwmOutputController ?? throw new ArgumentNullException(nameof(pwmOutputController));
        foreach (var pin in pinDefinitions.AllPins)
        {
            _pins[pin.Name] = pin;
        }
    }

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var parsedArguments = ParseArguments<Arguments>(context.Arguments, context.Log);
        if (parsedArguments == null)
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!_pins.TryGetValue(parsedArguments.PinName, out var pin))
        {
            var message = $"No known pin with the name '{parsedArguments.PinName}'";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var pwmPort = _pwmOutputController.CreatePwmPort(pin,
            new Frequency(parsedArguments.Frequency, Frequency.UnitType.Hertz), 0f);

        pwmPort.Start();

        context.Outputs.Add(parsedArguments.Variable.VariableName, pwmPort);

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }

    private class Arguments
    {
        public string PinName { get; set; }
        public int Frequency { get; set; }
        public VariableReferenceArgumentValue Variable { get; set; }
    }
}