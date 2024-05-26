using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Spi;

public class InitSpiBusOperation : OperationBase
{
    public const string OpCodeValue = "init_spi_bus";
    public const string StorageVariable = "StorageVariable";
    public const string SpiBusId = "SpiBusId";
    public const string Frequency = "Frequency";

    private readonly ISpiController _spiController;
    private readonly PinLookup _pins;

    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(StorageVariable, ParameterType.VariableReference, "Variable to store the spi bus in"),
        new OperationParameter(Frequency, ParameterType.Integer, "Frequency in kilohertz"),
        new OperationParameter(SpiBusId, ParameterType.Integer, "Id of the spi bus to create"),
    };

    public InitSpiBusOperation(ISpiController spiController, PinLookup pins)
    {
        _spiController = spiController ?? throw new ArgumentNullException(nameof(spiController));
        _pins = pins;
    }
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var variable = context.ParseVariableArgument(StorageVariable);
        if (variable == null)
        {
            context.LogInvalidRequiredArgument(StorageVariable, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var frequency = context.ParseIntArgument(Frequency);
        if (frequency is null or <= 0)
        {
            context.LogInvalidRequiredArgument(Frequency, ParameterType.Integer);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var spiBusId = context.ParseIntArgument(SpiBusId) ?? 3;
        IPin? clk, cipo, copi;

        if (spiBusId == 3)
        {
            clk = _pins.Get("SCLK") ?? _pins.Get("SPI3_SCK");
            cipo = _pins.Get("CIPO") ?? _pins.Get("SPI3_CIPO");
            copi = _pins.Get("COPI") ?? _pins.Get("SPI3_COPI");
        }
        else
        {
            clk = _pins.Get($"SPI{spiBusId}_SCK");
            cipo = _pins.Get($"SPI{spiBusId}_CIPO");
            copi = _pins.Get($"SPI{spiBusId}_COPI");
        }
        
        if (clk == null || cipo == null || copi == null)
        {
            context.Log(LogLevel.Error, $"Could not find pins for SPI bus {spiBusId}");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var spiBus = _spiController.CreateSpiBus(clk, copi, cipo,
            new Frequency(frequency.Value, global::Meadow.Units.Frequency.UnitType.Kilohertz));
        
        context.Outputs[variable.Value.VariableName] = spiBus;
        context.Log(LogLevel.Info, $"Created SPI bus {spiBusId} with frequency {frequency}kHz and stored in {variable.Value.VariableName}");

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}