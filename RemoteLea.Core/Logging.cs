using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteLea.Core;

public enum LogLevel { Debug, Info, Warning, Error }

/// <summary>
/// A function that can log a message of a specified level
/// </summary>
public delegate void LogFunction(LogLevel level, int instructionIndex, string operationName, string message);

/// <summary>
/// A function that logs a message scoped to a specific instruction
/// </summary>
public delegate void InstructionLogFunction(LogLevel level, string message);

public static class LogUtils
{
    /// <summary>
    /// Logs an error that a required argument was not provided
    /// </summary>
    public static void InvalidRequiredArgument(
        this InstructionLogFunction logger, 
        string argumentName,
        IReadOnlyDictionary<string, IArgumentValue> arguments,
        ParameterType allowedTypes)
    {
        if (arguments.TryGetValue(argumentName, out var argumentValue))
        {
            logger.IncorrectArgumentType(argumentName, argumentValue.GetType(), allowedTypes);
        }
        else
        {
            logger(LogLevel.Error, $"Argument `{argumentName}` not provided but is required");
        }
    }

    /// <summary>
    /// Logs an error that an argument was provided with an unexpected type
    /// </summary>
    public static void IncorrectArgumentType(
        this InstructionLogFunction logger, 
        string argumentName, 
        Type argumentType, 
        ParameterType allowedTypes)
    {
        // This is probably slow, but it's ok since it's probably an error case
        var validTypeString = Enum.GetValues(typeof(ParameterType))
            .Cast<Enum>()
            .Where(allowedTypes.HasFlag)
            .Select(x => x.ToString())
            .Aggregate((x, y) => $"{x}, {y}");

        var message = $"Argument `{argumentName}` had an unsupported type of `{argumentType.Name}`. Valid types are: {validTypeString}";
        logger(LogLevel.Error, message);
    }

    /// <summary>
    /// Logs an error if a variable was referenced that does not exist
    /// </summary>
    public static void ReferencedVariableDoesntExist(this InstructionLogFunction logger, string variableName)
    {
        var message = $"Variable with the name `{variableName}` was referenced but does not currently exist";
        logger(LogLevel.Error, message);
    }
}