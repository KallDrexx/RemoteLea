using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.PinIo;

[Operation(OpCode)]
[OperationParameter(0, PinNameParam, ParameterType.String, "Name of the pin")]
[OperationParameter(1, InterruptModeParam, ParameterType.String, 
    "Type of interrupt the pin should have. Values are 'None', 'EdgeRising', 'EdgeFalling', 'EdgeBoth'")]
[OperationParameter(2, StorageVariableParam, ParameterType.VariableReference, "Variable to store the pin in")]
public class InitInterruptPortOperation : OperationBase
{
    public const string OpCode = "init_int_port";
    public const string PinNameParam = "PinName";
    public const string InterruptModeParam = "InterruptMode";
    public const string StorageVariableParam = "Variable";

    private readonly IDigitalInterruptController _interruptController;
    private readonly PinLookup _pinLookup;

    public InitInterruptPortOperation(IDigitalInterruptController interruptController, PinLookup pinLookup)
    {
        _interruptController = interruptController;
        _pinLookup = pinLookup;
    }

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var pinName = context.ParseStringArgument(PinNameParam);
        if (pinName == null)
        {
            context.LogInvalidRequiredArgument(PinNameParam, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var variable = context.ParseVariableArgument(StorageVariableParam);
        if (variable == null)
        {
            context.LogInvalidRequiredArgument(StorageVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var interruptModeString = context.ParseStringArgument(InterruptModeParam);
        if (interruptModeString == null)
        {
            context.LogInvalidRequiredArgument(InterruptModeParam, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var interruptMode = interruptModeString.Trim() switch
        {
            "none" => InterruptMode.None,
            "edgerising" => InterruptMode.EdgeRising,
            "edgefalling" => InterruptMode.EdgeFalling,
            "edgeboth" => InterruptMode.EdgeBoth,
            _ => (InterruptMode?) null,
        };
        
        if (interruptMode == null)
        {
            context.Log(LogLevel.Error, $"Interrupt mode of '{interruptModeString}' is invalid, allowed " +
                                        "values are: 'None', 'EdgeRising', 'EdgeFalling', 'EdgeBoth'.");
        
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var pin = _pinLookup.Get(pinName);
        if (pin == null)
        {
            context.Log(LogLevel.Error, $"The specified pin '{pinName}' does not exist on this device");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var port = _interruptController.CreateDigitalInterruptPort(pin, interruptMode.Value);
        context.Outputs[variable.Value.VariableName] = port;

        context.Log(LogLevel.Debug,
            $"Digital interrupt port created for pin ${pinName} to variable '${variable.Value.VariableName}'");

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}