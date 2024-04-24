using System;

namespace RemoteLea.Serialization.AsmStyle;

public class InstructionDeserializationException : Exception
{
    public int Line { get; }
    public int CharIndex { get; }
    
    public InstructionDeserializationException(int line, int charIndex, string message) : base(message)
    {
        Line = line;
        CharIndex = charIndex;
    }
}