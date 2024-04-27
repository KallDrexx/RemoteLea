using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Tests.Core;

public class TestExecutionEngine : ExecutionEngine
{
    public Dictionary<string, object> VariableData => Variables;

    public TestExecutionEngine() : base(CreateOperationManager(), LogFunction)
    {
    }

    private static OperationManager CreateOperationManager()
    {
        var operations = typeof(OperationBase).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => x.IsAssignableTo(typeof(OperationBase)))
            .Select(x => Activator.CreateInstance(x))
            .Cast<OperationBase>()
            .ToArray();

        var manager = new OperationManager();
        foreach (var operation in operations)
        {
            manager.Register(operation);
        }

        return manager;
    }

    private static void LogFunction(LogLevel level, int instructionIndex, string operationName, string message)
    {
    }
}