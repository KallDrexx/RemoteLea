using System;

namespace RemoteLea.Core.Operations;

[AttributeUsage(AttributeTargets.Class)]
public class OperationAttribute : Attribute
{
    public string OpCode { get; }

    public OperationAttribute(string opCode)
    {
        OpCode = opCode;
    }
}