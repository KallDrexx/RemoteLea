using RemoteLea.Core.Operations.Implementations;
using Shouldly;

namespace RemoteLea.Core.Tests;

public class ExecutionEngineTests
{
    [Fact]
    public async Task Output_Value_Set_As_Variable_Value()
    {
        var engine = new TestExecutionEngine();
        var set = new InstructionSet(new List<Instruction>
        {
            new(SetValueOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { SetValueOperation.ValueParam, new IntArgumentValue(1234) },
                { SetValueOperation.VariableOutput, new VariableReferenceArgumentValue("test") },
            })
        });

        await engine.Execute(set);

        var variables = engine.VariableData;
        variables.ShouldContainKey("test");
        variables["test"].ShouldBe(1234);
    }
}