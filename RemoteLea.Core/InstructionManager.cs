using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteLea.Core;

/// <summary>
/// Allows registration and resolution of instruction implementations
/// </summary>
public class InstructionManager
{
    public enum RegistrationResult
    {
        Success = 0,
        DuplicateOpCode = 1
    }

    private record InstructionInfo(InstructionDefinition Definition, IInstruction Implementation);

    private readonly Dictionary<string, InstructionInfo> _instructions = new();

    /// <summary>
    /// Retrieves all registered instruction definitions
    /// </summary>
    public IReadOnlyList<InstructionDefinition> Definitions => _instructions.Values.Select(x => x.Definition).ToArray();

    /// <summary>
    /// Registers the specified instruction
    /// </summary>
    public RegistrationResult Register(IInstruction instruction)
    {
        if (instruction == null) throw new ArgumentNullException(nameof(instruction));

        var definition = instruction.Definition;
        if (!_instructions.TryAdd(definition.OpCode, new InstructionInfo(definition, instruction)))
        {
            return RegistrationResult.DuplicateOpCode;
        }

        return RegistrationResult.Success;
    }

    /// <summary>
    /// Retrieves an instruction implementation based on the provided opcode. Returns `null` if no instruction
    /// has been registered for the specified opcode.
    /// </summary>
    public IInstruction? Resolve(string opCode)
    {
        if (_instructions.TryGetValue(opCode, out var info))
        {
            return info.Implementation;
        }

        return null;
    }
}