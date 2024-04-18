using System.Collections;
using System.Collections.Generic;

namespace RemoteLea.Core;

/// <summary>
/// Description for how to run a specific instruction
/// </summary>
/// <param name="OpCode">The op code for the desired instruction to run</param>
/// <param name="Arguments">Named arguments being executed</param>
/// <param name="OutputVariableNames">Names of the variables to save each output to</param>
public record ExecutableInstruction(
    string OpCode,
    IReadOnlyDictionary<string, string> Arguments,
    IReadOnlyDictionary<string, string> OutputVariableNames);

/// <summary>
/// A set of instructions that can be executed in order
/// </summary>
public class InstructionSet : IEnumerator<ExecutableInstruction>
{
    public bool MoveNext()
    {
        throw new System.NotImplementedException();
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public ExecutableInstruction Current { get; }

    object IEnumerator.Current => Current;
}