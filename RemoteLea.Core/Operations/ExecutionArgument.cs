using System;

namespace RemoteLea.Core.Operations;

[AttributeUsage(AttributeTargets.Property)]
public class ExecutionArgument : Attribute
{
    public string? NameOverride { get; }
    public bool IsRequired { get; }
    
    public ExecutionArgument(string? nameOverride = null, bool isRequired = true)
    {
        NameOverride = nameOverride;
        IsRequired = isRequired;
    }
}