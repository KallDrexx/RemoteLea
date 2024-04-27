using System;
using System.Collections.Generic;
using System.Linq;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Serialization.AsmStyle;

/// <summary>
/// Provides functionality to serialize instructions into an assembly style text format,
/// and deserialize that style text format into instructions.
///
/// The text format has each instruction being a single line in the format of
/// "label: opcode arg1 arg2 argN # comment". The rules to keep in mind are:
///
/// * The `label` is optional and only required if an instruction should be labelled. Any
///   line with a label must have a colon to separate the label and the opcode
/// * The arguments provided must be in the same order they are defined for the operation
/// * Byte array arguments should be hex values starting with `0x`
/// * String values must be enclosed in quotes.
/// * Boolean values must be the literals `true` or `false`
/// * Referenced variable names are prefixed with `$` (e.g. `$myVar`)
/// * Op codes and labels are case-insensitive.
/// </summary>
public class InstructionSerializer
{
    private readonly OperationManager _operationManager;

    public InstructionSerializer(OperationManager operationManager)
    {
        _operationManager = operationManager;
    }

    public InstructionSet Deserialize(string content)
    {
        var instructions = new List<Instruction>();
        var spanContent = content.AsSpan();
        var lineNumber = 0;
        while (true)
        {
            var (instruction, nextLineStartIndex) = ParseInstructionLine(spanContent, lineNumber);
            if (instruction != null)
            {
                instructions.Add(instruction);
            }

            if (nextLineStartIndex >= spanContent.Length)
            {
                break;
            }

            spanContent = spanContent.Slice(nextLineStartIndex);
            lineNumber++;
        }

        return new InstructionSet(instructions);
    }

    private static string GetParameterValidTypeString(OperationParameter parameter)
    {
        return Enum.GetValues(typeof(ParameterType))
            .Cast<Enum>()
            .Where(parameter.ValidTypes.HasFlag)
            .Select(x => x.ToString())
            .Aggregate((x, y) => $"{x}, {y}");
    }

    private (Instruction?, int nextLineStartIndex) ParseInstructionLine(ReadOnlySpan<char> lineContent, int lineNumber)
    {
        var label = (string?)null;
        string? opCode;

        // Get first token
        var (tokenStart, tokenEnd) = GetNextTokenIndexes(lineContent, 0);
        if (tokenStart == null)
        {
            // Empty line
            return (null, tokenEnd);
        }

        if (lineContent[tokenEnd] == ':')
        {
            label = lineContent[tokenStart.Value..tokenEnd].ToString();

            // Next token should be the opcode
            (tokenStart, tokenEnd) = GetNextTokenIndexes(lineContent, tokenEnd + 1);
            if (tokenStart == null)
            {
                var message = "Label without opcode encountered. All labels must be attached to an instruction";
                throw new InstructionDeserializationException(lineNumber, 0, message);
            }

            opCode = lineContent[tokenStart.Value..(tokenEnd+1)].ToString();
        }
        else
        {
            // No colon means this is an opcode
            opCode = lineContent[tokenStart.Value..(tokenEnd+1)].ToString();
        }

        var definition = _operationManager.Resolve(opCode)?.Definition;
        if (definition == null)
        {
            var message = $"Unknown opcode '{opCode}'";
            throw new InstructionDeserializationException(lineNumber, tokenStart.Value, message);
        }

        var orderedParameters = definition.Parameters.OrderBy(x => x.Order).ToArray();
        var arguments = new Dictionary<string, IArgumentValue>();
        var argumentIndex = -1;
        while (true)
        {
            argumentIndex++;

            (tokenStart, tokenEnd) = GetNextTokenIndexes(lineContent, tokenEnd + 1);
            if (tokenStart == null)
            {
                // No more tokens in this line
                break;
            }

            if (argumentIndex >= orderedParameters.Length)
            {
                var message = $"Argument #{argumentIndex} does not match any parameters for {opCode}";
                throw new InstructionDeserializationException(lineNumber, tokenStart.Value, message);
            }

            var parameter = orderedParameters[argumentIndex];
            ParseArgument(lineContent, lineNumber, tokenStart.Value, tokenEnd, parameter, arguments);
        }

        var instruction = new Instruction(opCode, arguments, label);
        return (instruction, tokenEnd + 1);
    }

    private static void ParseArgument(
        ReadOnlySpan<char> lineContent,
        int lineNumber,
        int tokenStart,
        int tokenEnd,
        OperationParameter parameter,
        Dictionary<string, IArgumentValue> arguments)
    {
        var tokenSlice = lineContent.Slice(tokenStart, tokenEnd - tokenStart + 1);
        if (tokenSlice[0] == '$')
        {
            if (!parameter.ValidTypes.HasFlag(ParameterType.VariableReference))
            {
                var message = $"Argument is a variable reference, but that parameter " +
                              $"does not take that. Valid types are: {GetParameterValidTypeString(parameter)}";
                throw new InstructionDeserializationException(lineNumber, tokenStart, message);
            }

            if (tokenSlice.Length == 1)
            {
                var message = "Argument is a no-named variable reference";
                throw new InstructionDeserializationException(lineNumber, tokenStart, message);
            }

            var name = tokenSlice.Slice(1).ToString();
            arguments.Add(parameter.Name, new VariableReferenceArgumentValue(name));
            return;
        }

        var isAllDigits = true;
        foreach (var character in tokenSlice)
        {
            if (!char.IsDigit(character))
            {
                isAllDigits = false;
                break;
            }
        }

        if (isAllDigits && parameter.ValidTypes.HasFlag(ParameterType.Integer))
        {
            var value = Convert.ToInt32(tokenSlice.ToString());
            arguments.Add(parameter.Name, new IntArgumentValue(value));
            return;
        }

        if (parameter.ValidTypes.HasFlag(ParameterType.Bool))
        {
            if (tokenSlice.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                arguments.Add(parameter.Name, new BoolArgumentValue(true));
                return;
            }

            if (tokenSlice.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                arguments.Add(parameter.Name, new BoolArgumentValue(false));
                return;
            }
        }

        if (tokenSlice.StartsWith("0x") &&
            tokenSlice.Length > 2 &&
            parameter.ValidTypes.HasFlag(ParameterType.ByteArray))
        {
            // Convert from a hex string to byte array
            tokenSlice = tokenSlice.Slice(2);
            if (tokenSlice.Length % 2 != 0)
            {
                var message = "Byte array argument has an odd number of hex characters";
                throw new InstructionDeserializationException(lineNumber, tokenStart, message);
            }

            var byteArray = new byte[tokenSlice.Length / 2];
            for (var x = 0; x < tokenSlice.Length; x += 2)
            {
                if (!byte.TryParse(tokenSlice.Slice(x, 2), out var value))
                {
                    var message = "Invalid hex value in byte array";
                    throw new InstructionDeserializationException(lineNumber, tokenStart + 2 + x, message);
                }

                byteArray[x / 2] = value;
            }

            arguments.Add(parameter.Name, new ByteArrayArgumentValue(byteArray));
            return;
        }

        // Otherwise treat it as a string
        if (!parameter.ValidTypes.HasFlag(ParameterType.String))
        {
            var message = $"String value not allowed for '{parameter.Name}' parameter";
            throw new InstructionDeserializationException(lineNumber, tokenStart, message);
        }

        if (tokenSlice.StartsWith("\"") && tokenSlice.EndsWith("\"") && tokenSlice.Length >= 3)
        {
            tokenSlice = tokenSlice.Slice(1, tokenSlice.Length - 2);
        }

        arguments.Add(parameter.Name, new StringArgumentValue(tokenSlice.ToString()));
    }

    private static (int? start, int end) GetNextTokenIndexes(ReadOnlySpan<char> lineContent, int startIndex)
    {
        var tokenStart = (int?)null;
        var isQuoted = false;
        var escapeCharEncountered = false;

        for (var index = startIndex; index < lineContent.Length; index++)
        {
            if (lineContent[index] == '\n')
            {
                if (tokenStart == null)
                {
                    return (null, index); // no next token
                }

                // otherwise the last token was the one before the newline
                return (tokenStart.Value, index - 1);
            }

            if (char.IsWhiteSpace(lineContent[index]) && !isQuoted)
            {
                if (tokenStart == null)
                {
                    continue;
                }

                // Otherwise we hit the next space after the last token character
                return (tokenStart.Value, index - 1);
            }

            // A non-white space character hit
            if (tokenStart == null)
            {
                tokenStart = index;
                if (lineContent[index] == '"')
                {
                    isQuoted = true;
                }

                continue;
            }

            // Escape characters only valid in quoted strings
            if (isQuoted && lineContent[index] == '\\' && !escapeCharEncountered)
            {
                escapeCharEncountered = true;
                continue;
            }

            if (lineContent[index] == '"' && !escapeCharEncountered)
            {
                // This is the end of the quoted string
                return (tokenStart.Value, index);
            }

            if (escapeCharEncountered)
            {
                escapeCharEncountered = false;
            }
        }

        return (tokenStart, lineContent.Length - 1);
    }
}