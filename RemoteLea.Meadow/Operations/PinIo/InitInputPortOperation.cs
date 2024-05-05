using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.PinIo;

[Operation(OpCode)]
[OperationParameter(0, PinNameParam, ParameterType.String, "Name of the pin")]
[OperationParameter(1, StorageVariableParam, ParameterType.VariableReference, "Variable to store the pin in")]
public class InitInputPortOperation : OperationBase
{
    public const string OpCode = "init_input_port";
    public const string PinNameParam = "PinName";
    public const string StorageVariableParam = "Variable";

    private readonly IDigitalInputController _inputController;
    private readonly PinLookup _pinLookup;

    public InitInputPortOperation(IDigitalInputController inputController, PinLookup pinLookup)
    {
        _inputController = inputController ?? throw new ArgumentNullException(nameof(inputController));
        _pinLookup = pinLookup ?? throw new ArgumentNullException(nameof(pinLookup));
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

        var pin = _pinLookup.Get(pinName);
        if (pin == null)
        {
            context.Log(LogLevel.Error, $"The specified pin '{pinName}' does not exist on this device");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var port = _inputController.CreateDigitalInputPort(pin);
        context.Outputs[variable.Value.VariableName] = port;

        context.Log(LogLevel.Debug,
            $"Digital input port created for pin ${pinName} to variable '${variable.Value.VariableName}'");

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}