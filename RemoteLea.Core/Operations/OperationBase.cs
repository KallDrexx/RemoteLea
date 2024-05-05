using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RemoteLea.Core.Operations;

/// <summary>
/// Defines a parameter that can be given to an operation
/// </summary>
public record OperationParameter(string Name, ParameterType ValidTypes, string Description);

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
    /// <summary>
    /// String that uniquely identifies this operation
    /// </summary>
    protected abstract string OpCode { get; }

    /// <summary>
    /// Defines all parameters that this operation accepts. The order of these parameters
    /// determines the order in which they should be provided when calling the operation.
    /// </summary>
    protected abstract IReadOnlyList<OperationParameter> Parameters { get; }
    
    /// <summary>
    /// Retrieves the definition for this instruction
    /// </summary>
    public OperationDefinition Definition { get; }

    protected OperationBase()
    {
        // WARNING: Operation implementations must have opcodes and parameters set up
        // in their initializers (not constructors) to ensure they are available for 
        // the `OperationBase` constructor. Since the `OperationBase` constructor is called
        // prior to derived constructors, implementation classes can't contain their opcode/param
        // creation in their constructors.
        //
        // This is acceptable because these values should not be dynamic and should be known
        // at compile time.  That makes the virtual member calls safe.
        
        Definition = new OperationDefinition(OpCode, Parameters);
    }
   
    /// <summary>
    /// Executes the operation with the specified context
    /// </summary>
    public ValueTask<OperationExecutionResult> ExecuteAsync(IOperationExecutionContext context)
    {
        return ExecuteInternalAsync(context);
    }

    protected abstract ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context);
}