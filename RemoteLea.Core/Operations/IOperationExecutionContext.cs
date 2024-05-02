using System;
using System.Collections.Generic;
using System.Threading;

namespace RemoteLea.Core.Operations;

/// <summary>
/// All information needed for an operation to execute
/// </summary>
public interface IOperationExecutionContext
{
    IReadOnlyDictionary<string, IArgumentValue> Arguments { get; }
    IReadOnlyDictionary<string, object> Variables { get; }
    Dictionary<string, object> Outputs { get; }
    InstructionLogFunction Log { get; }
    CancellationToken CancellationToken { get; }
}

public static class OperationExecutionContextUtilities
{
    /// <summary>
    /// Logs an error that a required argument was not provided
    /// </summary>
    public static void LogInvalidRequiredArgument(
        this IOperationExecutionContext context,
        string argumentName,
        ParameterType allowedTypes)
    {
        if (context.Arguments.TryGetValue(argumentName, out var argumentValue))
        {
            context.Log.IncorrectArgumentType(argumentName, argumentValue.GetType(), allowedTypes);
        }
        else
        {
            context.Log(LogLevel.Error, $"Argument `{argumentName}` not provided but is required");
        }
    }
    
    public static int? ParseIntArgument(this IOperationExecutionContext context, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!context.Arguments.TryGetValue(name, out var argument) || argument is not IntArgumentValue intArg)
        {
            return null;
        }

        return intArg.Value;
    }

    public static bool? ParseBoolArgument(this IOperationExecutionContext context, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!context.Arguments.TryGetValue(name, out var argument) || argument is not BoolArgumentValue boolArg)
        {
            return null;
        }

        return boolArg.Value;
    }

    public static string? ParseStringArgument(this IOperationExecutionContext context, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!context.Arguments.TryGetValue(name, out var argument) || argument is not StringArgumentValue stringArg)
        {
            return null;
        }

        return stringArg.Value;
    }

    public static byte[]? ParseByteArrayArgument(this IOperationExecutionContext context, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!context.Arguments.TryGetValue(name, out var argument) || argument is not ByteArrayArgumentValue byteArg)
        {
            return null;
        }

        return byteArg.Value;
    }

    public static VariableReferenceArgumentValue? ParseVariableArgument(
        this IOperationExecutionContext context,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!context.Arguments.TryGetValue(name, out var argument) ||
            argument is not VariableReferenceArgumentValue variableArg)
        {
            return null;
        }

        return variableArg;
    }
}