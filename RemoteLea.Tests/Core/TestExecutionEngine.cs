using RemoteLea.Core;

namespace RemoteLea.Tests.Core;

public class TestExecutionEngine : ExecutionEngine
{
    public Dictionary<string, object> VariableData => Variables;

    public TestExecutionEngine() : base(Utilities.GetOperationManagerWithAllOperations(), LogFunction)
    {
    }

    private static void LogFunction(LogLevel level, int instructionIndex, string operationName, string message)
    {
    }
}