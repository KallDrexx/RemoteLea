using System;
using System.Linq;

namespace RemoteLea.Core;

public enum LogLevel { Info, Error }

/// <summary>
/// A function that can log a message of a specified level
/// </summary>
public delegate void LogFunction(LogLevel level, string operationName, string message);

public static class LogUtils
{
    /// <summary>
    /// Logs an error that a required argument was not provided
    /// </summary>
    public static void RequiredArgumentNotProvided(this LogFunction logger, string operationName, string argumentName)
    {
        logger(LogLevel.Error, operationName, $"Argument `{argumentName}` not provided but is required");
    }

    /// <summary>
    /// Logs an error that an argument was provided with an unexpected type
    /// </summary>
    public static void IncorrectArgumentType(
        this LogFunction logger, 
        string operationName,
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
        logger(LogLevel.Error, operationName, message);
    }
}