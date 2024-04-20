using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteLea.Core.Operations;

/// <summary>
/// Allows registration and resolution of operation implementations
/// </summary>
public class OperationManager
{
    public enum RegistrationResult
    {
        Success = 0,
        DuplicateOpCode = 1
    }

    private record OperationInfo(OperationDefinition Definition, OperationBase Implementation);

    private readonly Dictionary<string, OperationInfo> _instructions = new();

    /// <summary>
    /// Retrieves all registered instruction definitions
    /// </summary>
    public IReadOnlyList<OperationDefinition> Definitions => _instructions.Values.Select(x => x.Definition).ToArray();

    /// <summary>
    /// Registers the specified operation
    /// </summary>
    public RegistrationResult Register(OperationBase operation)
    {
        if (operation == null) throw new ArgumentNullException(nameof(operation));

        var definition = operation.Definition;
        return _instructions.TryAdd(definition.OpCode, new OperationInfo(definition, operation))
            ? RegistrationResult.Success
            : RegistrationResult.DuplicateOpCode;
    }

    /// <summary>
    /// Retrieves an operation implementation based on the provided opcode. Returns `null` if no instruction
    /// has been registered for the specified opcode.
    /// </summary>
    public OperationBase? Resolve(string opCode)
    {
        return _instructions.TryGetValue(opCode, out var info) 
            ? info.Implementation 
            : null;
    }
}