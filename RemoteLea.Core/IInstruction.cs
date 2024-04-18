using System.Collections.Generic;

namespace RemoteLea.Core;

/// <summary>
/// Defines a parameter that can be given to an instruction
/// </summary>
public record InstructionParameter(string Name, string Description);

/// <summary>
/// Defines a value that is created by an instruction and can be placed in a variable for
/// later reference.
/// </summary>
public record InstructionOutput(string Name, string Description);

/// <summary>
/// Defines a specific instruction that's been implemented
/// </summary>
/// <param name="OpCode">String that uniquely identifies a single instruction</param>
/// <param name="Parameters">Named parameters for each instruction</param>
/// <param name="Outputs">Named outputs that are saved</param>
public record InstructionDefinition(string OpCode, IReadOnlyList<string> Parameters, IReadOnlyList<string> Outputs);

/// <summary>
/// An instruction contains execution logic for performing a type of operation. Each implementation
/// is intended to be stateless and called each time the instruction's opcode is encountered.
/// </summary>
public interface IInstruction
{
    /// <summary>
    /// Retrieves the definition for this instruction
    /// </summary>
    InstructionDefinition Definition { get; }
}