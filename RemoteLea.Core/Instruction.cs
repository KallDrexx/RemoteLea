using System.Collections.Generic;

namespace RemoteLea.Core;

/// <summary>
/// Describes how to execute an operation
/// </summary>
/// <param name="OpCode">The identifier of the operation to execute</param>
/// <param name="Arguments">The parameter names and their values</param>
/// <param name="Label">A label that points to this specific instruction</param>
public record Instruction(string OpCode, IReadOnlyDictionary<string, IArgumentValue> Arguments, string? Label = null);