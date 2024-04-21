using RemoteLea.Core.Operations;
using RemoteLea.Core.Operations.Implementations;
using Shouldly;

namespace RemoteLea.Core.Tests;

public class ExecutionEngineTests
{
    [Fact]
    public async Task Output_Value_Set_As_Variable_Value()
    {
        var engine = CreateEngine();
        var set = new InstructionSet(new List<Instruction>
        {
            new(SetValueOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { SetValueOperation.ValueParam, new IntArgumentValue(1234) },
                { SetValueOperation.VariableOutput, new VariableReferenceArgumentValue("test") },
            })
        });

        var result = await engine.Execute(set);

        result.Keys.ShouldContain("test");
        result["test"].ShouldBe(1234);
    }

    private ExecutionEngine CreateEngine()
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

        return new ExecutionEngine(manager, LogFunction);

        void LogFunction(LogLevel level, int instructionIndex, string operationName, string message)
        {
        }
    }
}