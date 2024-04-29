using System.Collections.Generic;
using RemoteLea.Core.Operations.Implementations;

namespace RemoteLea.Core.Operations;

public static class CoreOperations
{
    public static IEnumerable<OperationBase> All()
    {
        yield return new AddOperation();
        yield return new DelayOperation();
        yield return new JumpOperation();
        yield return new JumpIfEqualOperation();
        yield return new JumpIfNotEqualOperation();
        yield return new LogVarOperation();
        yield return new SetValueOperation();
    }
}