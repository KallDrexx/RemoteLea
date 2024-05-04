using System.Collections.Generic;
using Meadow;
using Meadow.Hardware;
using RemoteLea.Core.Operations;
using RemoteLea.Meadow.Operations.I2c;
using RemoteLea.Meadow.Operations.Pwm;

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
            yield return new I2CWriteOperation();
        }
    }
}