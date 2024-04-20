using System;

namespace RemoteLea.Core.Operations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class OperationParameterAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public ParameterType ValidTypes { get; }
    
    public OperationParameterAttribute(string name, ParameterType validTypes, string description)
    {
        Name = name;
        Description = description;
        ValidTypes = validTypes;
    }
}