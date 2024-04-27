using RemoteLea.Core;
using RemoteLea.Core.Operations.Implementations;
using Shouldly;

namespace RemoteLea.Tests.Core;

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

        await engine.Execute(set).WaitAsync(TimeSpan.FromMilliseconds(100));
        var variables = engine.VariableData;
        variables.ShouldContainKey("test");
        variables["test"].ShouldBe(1234);
    }

    [Fact]
    public async Task Multiple_Instructions()
    {
        var engine = new TestExecutionEngine();
        var set = new InstructionSet(new List<Instruction>
        {
            new(SetValueOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { SetValueOperation.ValueParam, new IntArgumentValue(10) },
                { SetValueOperation.VariableOutput, new VariableReferenceArgumentValue("value") },
            }),
            new(AddOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { AddOperation.ValueParam, new IntArgumentValue(5) },
                { AddOperation.OutputParam, new VariableReferenceArgumentValue("value") },
            })
        });

        await engine.Execute(set).WaitAsync(TimeSpan.FromMilliseconds(100));
        var variables = engine.VariableData;
        variables.ShouldContainKey("value");
        variables["value"].ShouldBe(15);
    }

    [Fact]
    public async Task Jump_Based_Looping()
    {
        var engine = new TestExecutionEngine();
        var set = new InstructionSet(new List<Instruction>
        {
            new(SetValueOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { SetValueOperation.ValueParam, new IntArgumentValue(0) },
                { SetValueOperation.VariableOutput, new VariableReferenceArgumentValue("value") },
            }),
            new(SetValueOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { SetValueOperation.ValueParam, new IntArgumentValue(15) },
                { SetValueOperation.VariableOutput, new VariableReferenceArgumentValue("check") },
            }),
            new(AddOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { AddOperation.ValueParam, new IntArgumentValue(1) },
                { AddOperation.OutputParam, new VariableReferenceArgumentValue("value") },
            }, "counter"),
            new(JumpIfNotEqualOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { JumpIfNotEqualOperation.FirstVar, new VariableReferenceArgumentValue("value") },
                { JumpIfNotEqualOperation.SecondVar, new VariableReferenceArgumentValue("check") },
                { JumpIfNotEqualOperation.LabelParam, new StringArgumentValue("counter") }
            }),
        });

        await engine.Execute(set).WaitAsync(TimeSpan.FromMilliseconds(100));
        var variables = engine.VariableData;
        variables.ShouldContainKey("value");
        variables["value"].ShouldBe(15);
    }

    [Fact]
    public async Task Can_Cancel_Infinite_Loop()
    {
        var engine = new TestExecutionEngine();
        var set = new InstructionSet(new List<Instruction>
        {
            new(SetValueOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { SetValueOperation.ValueParam, new IntArgumentValue(0) },
                { SetValueOperation.VariableOutput, new VariableReferenceArgumentValue("value") },
            }, "label"),
            new(JumpOperation.OpCode, new Dictionary<string, IArgumentValue>
            {
                { JumpOperation.LabelParam, new StringArgumentValue("label") },
            }),
        });

        var executionTask = engine.Execute(set);
        await Task.Delay(100);
        executionTask.IsCompleted.ShouldBeFalse("Engine should still be looping");
        
        engine.CancelCurrentExecution();
        await Task.Delay(10);
        executionTask.IsCompleted.ShouldBeTrue("Engine execution should have been stopped");
    }
}