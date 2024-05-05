using RemoteLea.Core;
using RemoteLea.Core.Operations.Implementations;
using RemoteLea.Serialization.AsmStyle;
using Shouldly;

namespace RemoteLea.Tests.Serialization;

public class InstructionSerializerTests
{
    [Fact]
    public void Can_Deserialize_Multiple_Instructions()
    {
        var input = """
                    set $var 12
                    some_label: add 5 $var
                    jump "some_label"
                    """;

        var operationManager = Utilities.GetOperationManagerWithAllOperations();
        var serializer = new InstructionSerializer(operationManager);

        var instructionSet = serializer.Deserialize(input);
        using var enumerator = instructionSet.GetEnumerator();
        enumerator.MoveNext().ShouldBeTrue();
        enumerator.Current.ShouldMatch(new Instruction(
            SetValueOperation.OpCodeValue,
            new Dictionary<string, IArgumentValue>
            {
                { SetValueOperation.VariableOutput, new VariableReferenceArgumentValue("var") },
                { SetValueOperation.ValueParam, new IntArgumentValue(12) },
            }));

        enumerator.MoveNext().ShouldBeTrue();
        enumerator.Current.ShouldMatch(new Instruction(
            AddOperation.OpCodeValue,
            new Dictionary<string, IArgumentValue>
            {
                { AddOperation.ValueParam, new IntArgumentValue(5) },
                { AddOperation.OutputParam, new VariableReferenceArgumentValue("var") },
            },
            "some_label"));

        enumerator.MoveNext().ShouldBeTrue();
        enumerator.Current.ShouldMatch(new Instruction(
            JumpOperation.OpCodeValue,
            new Dictionary<string, IArgumentValue>
            {
                {JumpOperation.LabelParam, new StringArgumentValue("some_label")}
            }));

        enumerator.MoveNext().ShouldBeFalse();
    }
}