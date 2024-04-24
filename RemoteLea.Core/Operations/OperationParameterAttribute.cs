using System;

namespace RemoteLea.Core.Operations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class OperationParameterAttribute : Attribute
{
    public int Order { get; }
    public string Name { get; }
    public string Description { get; }
    public ParameterType ValidTypes { get; }
    
    public OperationParameterAttribute(int order, string name, ParameterType validTypes, string description)
    {
        Order = order;
        Name = name;
        Description = description;
        ValidTypes = validTypes;
    }
}