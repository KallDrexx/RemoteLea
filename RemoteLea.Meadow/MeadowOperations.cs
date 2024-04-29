using System.Collections.Generic;
using Meadow;
using Meadow.Hardware;
using RemoteLea.Core.Operations;
using RemoteLea.Meadow.Operations.Pwm;

namespace RemoteLea.Meadow;

public static class MeadowOperations
{
    public static IEnumerable<OperationBase> All(IMeadowDevice device, IPinDefinitions pinDefinitions)
    {
        if (device is IPwmOutputController pwmController)
        {
            yield return new InitPwmOperation(pinDefinitions, pwmController);
            yield return new SetPwmDutyCycleOperation();
        }
    }
}