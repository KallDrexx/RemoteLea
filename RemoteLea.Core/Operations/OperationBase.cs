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
            parameters.Select(x => new OperationParameter(x.Name, x.ValidTypes, x.Description)).ToArray());
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

    protected T? ParseArguments<T>(IReadOnlyDictionary<string, IArgumentValue> arguments, InstructionLogFunction log)
        where T : class, new()
    {
        var result = new T();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            var executionArgAttribute = property.GetCustomAttribute<ExecutionArgument>();
            var name = executionArgAttribute?.NameOverride ?? property.Name;
            var isRequired = executionArgAttribute == null || executionArgAttribute.IsRequired;
            if (!arguments.TryGetValue(name, out var argumentValue))
            {
                if (isRequired)
                {
                    log.RequiredArgumentNotProvided(name);
                    return null;
                }
                
                continue;
            }

            if (property.PropertyType == typeof(int))
            {
                if (argumentValue is IntArgumentValue arg)
                {
                    property.SetValue(result, arg.Value);
                }
                else
                {
                    if (isRequired)
                    {
                        log.IncorrectArgumentType(name, argumentValue.GetType(), ParameterType.Integer);
                        return null;
                    }
                    
                    // Since it's not required, it's possible it's going to fill another property with a different
                    // type, so don't log it.
                    continue;
                }
            }
            else if (property.PropertyType == typeof(bool))
            {
                if (argumentValue is BoolArgumentValue arg)
                {
                    property.SetValue(result, arg.Value);
                }
                else
                {
                    if (isRequired)
                    {
                        log.IncorrectArgumentType(name, argumentValue.GetType(), ParameterType.Bool);
                        return null;
                    }
                    
                    // Since it's not required, it's possible it's going to fill another property with a different
                    // type, so don't log it.
                    continue;
                }
            }
            else if (property.PropertyType == typeof(string))
            {
                if (argumentValue is StringArgumentValue arg)
                {
                    property.SetValue(result, arg.Value);
                }
                else
                {
                    if (isRequired)
                    {
                        log.IncorrectArgumentType(name, argumentValue.GetType(), ParameterType.String);
                        return null;
                    }
                    
                    // Since it's not required, it's possible it's going to fill another property with a different
                    // type, so don't log it.
                    continue;
                }
            }
            else if (property.PropertyType == typeof(byte[]))
            {
                if (argumentValue is ByteArrayArgumentValue arg)
                {
                    property.SetValue(result, arg.Value);
                }
                else
                {
                    if (isRequired)
                    {
                        log.IncorrectArgumentType(name, argumentValue.GetType(), ParameterType.ByteArray);
                        return null;
                    }
                    
                    // Since it's not required, it's possible it's going to fill another property with a different
                    // type, so don't log it.
                    continue;
                }
            }
            else if (property.PropertyType == typeof(VariableReferenceArgumentValue))
            {
                if (argumentValue is VariableReferenceArgumentValue arg)
                {
                    property.SetValue(result, arg);
                }
                else
                {
                    if (isRequired)
                    {
                        log.IncorrectArgumentType(name, argumentValue.GetType(), ParameterType.VariableReference);
                        return null;
                    }
                    
                    // Since it's not required, it's possible it's going to fill another property with a different
                    // type, so don't log it.
                    continue;
                }
            }
            else
            {
                var message = $"Cannot fill argument for property of type {property.PropertyType.FullName}";
                throw new NotSupportedException(message);
            }
        }

        return result;
    }
}