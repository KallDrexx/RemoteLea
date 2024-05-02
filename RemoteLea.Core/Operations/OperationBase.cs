using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations;

/// <summary>
/// Defines a parameter that can be given to an operation
/// </summary>
public record OperationParameter(int Order, string Name, ParameterType ValidTypes, string Description);

/// <summary>
/// Defines a specific operation that's been implemented
/// </summary>
/// <param name="OpCode">String that uniquely identifies a single operation</param>
/// <param name="Parameters">Named parameters for each operation</param>
public record OperationDefinition(string OpCode, IReadOnlyList<OperationParameter> Parameters);

/// <summary>
/// Contains execution logic for performing a type of operation. Each implementation
/// is intended to be stateless and called each time the instruction's opcode is encountered.
/// </summary>
public abstract class OperationBase
{
    protected OperationBase()
    {
        var operationAttr = GetType().GetCustomAttribute<OperationAttribute>();
        if (operationAttr == null)
        {
            const string message = "Operation had no `OperationAttribute` specified";
            throw new InvalidOperationException(message);
        }

        var parameters = GetType().GetCustomAttributes<OperationParameterAttribute>();
        Definition = new OperationDefinition(
            operationAttr.OpCode,
            parameters.Select(x => new OperationParameter(x.Order, x.Name, x.ValidTypes, x.Description)).ToArray());
    }

    /// <summary>
    /// Retrieves the definition for this instruction
    /// </summary>
    public OperationDefinition Definition { get; }

    /// <summary>
    /// Executes the operation with the specified context
    /// </summary>
    public ValueTask<OperationExecutionResult> ExecuteAsync(IOperationExecutionContext context)
    {
        return ExecuteInternalAsync(context);
    }

    protected abstract ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context);
}