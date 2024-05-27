using System.Collections.Generic;
using Meadow;
using Meadow.Hardware;
using RemoteLea.Core.Operations;
using RemoteLea.Meadow.Operations.I2c;
using RemoteLea.Meadow.Operations.Mcp;
using RemoteLea.Meadow.Operations.PinIo;
using RemoteLea.Meadow.Operations.Pwm;
using RemoteLea.Meadow.Operations.Spi;

namespace RemoteLea.Meadow;

public static class MeadowOperations
{
    public static IEnumerable<OperationBase> All(IMeadowDevice device, PinLookup pins)
    {
        if (device is IPwmOutputController pwmController)
        {
            yield return new InitPwmOperation(pwmController, pins);
            yield return new SetPwmDutyCycleOperation();
        }

        if (device is II2cController i2CController)
        {
            yield return new InitI2CBus(i2CController);
        }

        if (device is IDigitalInputController inputController)
        {
            yield return new InitInputPortOperation(inputController, pins);
        }

        if (device is IDigitalOutputController outputController)
        {
            yield return new InitOutputPortOperation(outputController, pins);
        }

        if (device is ISpiController spiController)
        {
            yield return new InitSpiBusOperation(spiController, pins);
        }

        yield return new I2CWriteOperation();
        yield return new InitMcp23008Operation(pins);
        yield return new InitMcp23XInputOperation();
        yield return new InitMcp23XInterruptOperation();
        yield return new InitMcp23XOutputOperation();
        yield return new SaveInputStateOperation();
        yield return new SetOutputPortStateOperation();
        yield return new SpiReadOperation();
        yield return new SpiWriteOperation();
        yield return new WaitForInputPortStateOperation();
    }
}