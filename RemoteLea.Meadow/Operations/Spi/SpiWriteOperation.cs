using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Spi;

public class SpiWriteOperation : OperationBase
{
    public const string OpCodeValue = "spi_write";
    public const string BusVariable = "BusVariable";
    public const string ChipSelectVariable = "ChipSelectVariable";
    public const string DataToWrite = "DataToWrite";

    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(BusVariable, ParameterType.VariableReference,
            "Variable containing the SPI bus to write to"),

        new OperationParameter(ChipSelectVariable, ParameterType.VariableReference,
            "Variable containing the chip select digital output port to use"),
        
        new OperationParameter(DataToWrite, ParameterType.ByteArray, "Data to write"),
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
        
        var dataToWrite = context.ParseByteArrayArgument(DataToWrite);
        if (dataToWrite == null)
        {
            context.LogInvalidRequiredArgument(DataToWrite, ParameterType.ByteArray);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        spiBus.Write(chipSelect, dataToWrite);
        context.Log(LogLevel.Info, $"Wrote {BitConverter.ToString(dataToWrite)} to SPI bus");
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}