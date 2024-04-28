using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteLea.Core.Operations;

public static class CoreOperations
{
    public static IReadOnlyList<OperationBase> All => typeof(OperationBase).Assembly
        .GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => typeof(OperationBase).IsAssignableFrom(x))
        .Select(Activator.CreateInstance)
        .Cast<OperationBase>()
        .ToArray();
}