using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Mcp;

public class InitMcp23008Operation : OperationBase
{
    public const string OpCodeValue = "init_mcp23008";
    private const string I2cBusParam = "I2cBus";
    private const string AddressParam = "Address";
    private const string InterruptPinName = "InterruptPin";
    private const string ResetPinName = "ResetPin";
    private const string StorageVariableParam = "Variable";

    private readonly PinLookup _pinLookup;

    public InitMcp23008Operation(PinLookup pinLookup)
    {
        _pinLookup = pinLookup;
    }

    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(I2cBusParam, ParameterType.VariableReference,
            "The variable containing an initialized I2C bus to use"),
        
        new OperationParameter(AddressParam, ParameterType.ByteArray, "The address of the MCP23008"),
        new OperationParameter(InterruptPinName, ParameterType.String, "Pin name for mcp interrupt"),
        new OperationParameter(ResetPinName, ParameterType.String, "Pin name for mcp reset"),
        new OperationParameter(StorageVariableParam, ParameterType.VariableReference,
            "Variable to store the mcp instance in"),
    };
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var address = context.ParseByteArrayArgument(AddressParam);
        if (address == null)
        {
            context.LogInvalidRequiredArgument(AddressParam, ParameterType.ByteArray);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (address.Length != 1)
        {
            context.Log(LogLevel.Error, $"The address must be a single byte, but was {address.Length} bytes");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var i2CBusVariable = context.ParseVariableArgument(I2cBusParam);
        if (i2CBusVariable == null)
        {
            context.LogInvalidRequiredArgument(I2cBusParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (context.Variables.TryGetValue(i2CBusVariable.Value.VariableName, out var i2CBus) == false)
        {
            context.Log(LogLevel.Error, $"The specified I2C bus variable '{i2CBusVariable.Value.VariableName}' does not exist");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (i2CBus is not II2cBus i2C)
        {
            context.Log(LogLevel.Error, $"The specified I2C bus variable '{i2CBusVariable.Value.VariableName}' is not an I2C bus");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var storageVariable = context.ParseVariableArgument(StorageVariableParam);
        if (storageVariable == null)
        {
            context.LogInvalidRequiredArgument(StorageVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var interruptPinName = context.ParseStringArgument(InterruptPinName);
        if (interruptPinName == null)
        {
            context.LogInvalidRequiredArgument(InterruptPinName, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var interruptPin = _pinLookup.Get(interruptPinName);
        if (interruptPin == null)
        {
            context.Log(LogLevel.Error, $"The specified interrupt pin '{interruptPinName}' does not exist on this device");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var resetPinName = context.ParseStringArgument(ResetPinName);
        if (resetPinName == null)
        {
            context.LogInvalidRequiredArgument(ResetPinName, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var resetPin = _pinLookup.Get(resetPinName);
        if (resetPin == null)
        {
            context.Log(LogLevel.Error, $"The specified reset pin '{resetPinName}' does not exist on this device");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var interrupt = interruptPin.CreateDigitalInterruptPort(InterruptMode.EdgeRising, ResistorMode.InternalPullDown);
        var reset = resetPin.CreateDigitalOutputPort();
        
        var mcp = new Mcp23008(i2C, address[0], interrupt, reset);
        context.Outputs[storageVariable.Value.VariableName] = mcp;
        
        // Put interrupt and reset pins in a variable so it gets cleaned up on the next execution
        context.Outputs[$"__pin_{interruptPinName}"] = interruptPin;
        context.Outputs[$"__pin_{resetPinName}"] = resetPin;
        
        context.Log(LogLevel.Debug,
            $"MCP23008 created with address {address[0]} to variable '{storageVariable.Value.VariableName}'");
        
        Console.WriteLine($"MCP pins: {interruptPinName}, {resetPinName}");
        
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}