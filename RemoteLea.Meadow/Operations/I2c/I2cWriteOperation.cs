using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.I2c;

[Operation(OpCode)]
[OperationParameter(0, BusVariable, ParameterType.VariableReference, "Variable holding the i2c bus to write to")]
[OperationParameter(1, AddressParam, ParameterType.ByteArray, "Address to write to")]
[OperationParameter(2, DataParam, ParameterType.ByteArray, "Data to write on the i2c bus")]
public class I2CWriteOperation : OperationBase
{
    public const string OpCode = "i2cwrite";
    public const string BusVariable = "BusVariable";
    public const string AddressParam = "Address";
    public const string DataParam = "Data";
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var data = context.ParseByteArrayArgument(DataParam);
        if (data == null)
        {
            context.LogInvalidRequiredArgument(DataParam, ParameterType.ByteArray);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var address = context.ParseByteArrayArgument(AddressParam);
        if (address == null)
        {
            context.LogInvalidRequiredArgument(AddressParam, ParameterType.ByteArray);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (address.Length != 1)
        {
            context.Log(LogLevel.Error, $"Address was {address.Length} bytes but should only be one byte");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var busVariable = context.ParseVariableArgument(BusVariable);
        if (busVariable == null)
        {
            context.LogInvalidRequiredArgument(BusVariable, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (!context.Variables.TryGetValue(busVariable.Value.VariableName, out var busObject))
        {
            context.Log.ReferencedVariableDoesntExist(busVariable.Value.VariableName);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (busObject is not II2cBus i2CBus)
        {
            context.Log(LogLevel.Error, $"Variable '{busVariable.Value.VariableName}' is of type " +
                                        $"{busObject.GetType()} but II2cBus was expected");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        i2CBus.Write(address[0], data.AsSpan());
        context.Log(LogLevel.Debug, $"Wrote {BitConverter.ToString(data)} on I2C to address {BitConverter.ToString(address)}");

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}