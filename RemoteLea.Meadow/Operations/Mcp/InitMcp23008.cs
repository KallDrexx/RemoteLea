using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Mcp;

public class InitMcp23008 : OperationBase
{
    public const string OpCodeValue = "init_mcp23008";
    private const string AddressParam = "Address";  
    private const string I2cBusParam = "I2cBus";
    private const string StorageVariableParam = "Variable";
    
    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(AddressParam, ParameterType.ByteArray, "The address of the MCP23008"),
        new OperationParameter(I2cBusParam, ParameterType.VariableReference,
            "The variable containing an initialized I2C bus to use"),

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
        
        var mcp = new Mcp23008(i2C, address[0]);
        context.Outputs[storageVariable.Value.VariableName] = mcp;
        
        context.Log(LogLevel.Debug,
            $"MCP23008 created with address {address[0]} to variable '{storageVariable.Value.VariableName}'");
        
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}