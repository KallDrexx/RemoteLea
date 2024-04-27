using RemoteLea.Core;
using RemoteLea.Core.Operations;
using Shouldly;

namespace RemoteLea.Tests;

public static class Utilities
{
    public static IReadOnlyList<OperationBase> GetImplementedOperations()
    {
        return typeof(OperationBase).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => x.IsAssignableTo(typeof(OperationBase)))
            .Select(Activator.CreateInstance)
            .Cast<OperationBase>()
            .ToArray();
    }

    public static OperationManager GetOperationManagerWithAllOperations()
    {
        var operations = GetImplementedOperations();
        var manager = new OperationManager();
        foreach (var operation in operations)
        {
            manager.Register(operation);
        }

        return manager;
    }

    public static void ShouldMatch(this Instruction? found, Instruction expected)
    {
        found.ShouldNotBeNull();
        found.OpCode.ShouldBe(expected.OpCode);
        found.Label.ShouldBe(expected.Label);
        found.Arguments.ShouldNotBeNull();
        found.Arguments.Count.ShouldBe(expected.Arguments.Count);

        foreach (var (expectedKey, expectedValue) in expected.Arguments)
        {
            found.Arguments.Keys.ShouldContain(expectedKey);
            found.Arguments[expectedKey].ShouldBeEquivalentTo(expectedValue);
        }
    }
}