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
    private readonly record struct ParsedLine(string? Label, string OpCode, string[] Arguments);

    private readonly OperationManager _operationManager;

    public InstructionSerializer(OperationManager operationManager)
    {
        _operationManager = operationManager;
    }

    public List<Instruction> Deserialize(string content)
    {
        var memoryContent = content.AsMemory();
        var lineNumber = 0;
        while (true)
        {
            var (parsedLine, index) = ParseLine(memoryContent, lineNumber);
            if (parsedLine != null)
            {
                
            }

            if (memoryContent.Length <= index)
            {
                break;
            }

            memoryContent = memoryContent.Slice(index);
        }
    }

    private (ParsedLine?, int nextLineStartIndex) ParseLine(ReadOnlyMemory<char> lineContent, int lineNumber)
    {
        var strings = new List<ReadOnlyMemory<char>>();
        var colonEncountered = false;

        var span = lineContent.Span;
        var index = 0;
        var inComment = false;
        var newLineEncountered = false;
        while (index < lineContent.Length)
        {
            if (newLineEncountered)
            {
                break;
            }
            
            switch (span[index])
            {
                case ' ':
                    break;

                case '#':
                    inComment = true;
                    break;

                case '"':
                    if (!inComment)
                    {
                        var (startIndex, endIndex) = ParseQuote(span, index, lineNumber);
                        strings.Add(lineContent.Slice(startIndex, endIndex));
                    }

                    break;
                
                case '\n':
                    newLineEncountered = true;
                    break;
                
                case ':':
                    if (!inComment)
                    {
                        if (colonEncountered)
                        {
                            const string message = "Multiple colons encountered, only one after the label is allowed";
                            throw new InstructionDeserializationException(lineNumber, index, message);
                        }

                        colonEncountered = true;
                    }

                    break;
            }

            index++;
        }

        if (strings.Count == 0)
        {
            return (null, index);
        }

        if (colonEncountered && strings.Count == 1)
        {
            const string message = "Label with no op code encountered";
            throw new InstructionDeserializationException(lineNumber, 0, message);
        }

        var skipCount = colonEncountered ? 2 : 1;
        var arguments = strings.Skip(skipCount).Select(x => x.ToString()).ToArray();
        var parsedLine = new ParsedLine(
            colonEncountered ? strings[0].ToString() : null,
            colonEncountered ? strings[1].ToString() : strings[0].ToString(),
            arguments);

        return (parsedLine, index);
    }

    private (int, int) ParseQuote(ReadOnlySpan<char> lineContent, int startIndex, int lineNum)
    {
        // Find an ending quote that's not preceded by a `\`
        var index = startIndex;
        var isEscaped = false;

        while (index < lineContent.Length)
        {
            index++;
            switch (lineContent[index])
            {
                case '\'':
                    isEscaped = true;
                    continue;

                case '"':
                    if (!isEscaped)
                    {
                        return (startIndex, index);
                    }

                    break;
            }
        }

        throw new InstructionDeserializationException(lineNum, startIndex, "Quoted string has no ending quote");
    }
}