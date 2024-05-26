using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Spi;

public class SpiReadOperation : OperationBase
{
    public const string OpCodeValue = "spi_read";
    public const string BusVariable = "BusVariable";
    public const string ChipSelectVariable = "ChipSelectVariable";
    public const string StorageVariable = "StorageVariable";
    public const string ByteCount = "ByteCount";

    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(BusVariable, ParameterType.VariableReference,
            "Variable containing the SPI bus to read from"),

        new OperationParameter(ChipSelectVariable, ParameterType.VariableReference,
            "Variable containing the chip select digital output port to use"),
        
        new OperationParameter(StorageVariable, ParameterType.VariableReference, 
            "Variable to store the read data in"), 
        
        new OperationParameter(ByteCount, ParameterType.Integer, "Number of bytes to read"),
    };
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        if (!context.TryGetVariableValueFromArgument(BusVariable, out ISpiBus spiBus))
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (!context.TryGetVariableValueFromArgument(ChipSelectVariable, out IDigitalOutputPort chipSelect))
        {
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var byteCount = context.ParseIntArgument(ByteCount);
        if (byteCount is null or <= 0)
        {
            context.LogInvalidRequiredArgument(ByteCount, ParameterType.Integer);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var storageVariable = context.ParseVariableArgument(StorageVariable);
        if (storageVariable == null)
        {
            context.LogInvalidRequiredArgument(StorageVariable, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var data = new byte[byteCount.Value];
        spiBus!.Read(chipSelect, data.AsSpan());
        
        context.Outputs[storageVariable.Value.VariableName] = data;
        
        var dataString = BitConverter.ToString(data);
        context.Log(LogLevel.Info, $"Read {byteCount} bytes from SPI bus: 0x{dataString}");

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}