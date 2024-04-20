using System;

namespace RemoteLea.Core;

/// <summary>
/// Represents the possible types a parameter can be represented as
/// </summary>
[Flags]
public enum ParameterType
{
    Unspecified = 0,
    Integer = 1 << 0,
    Bool = 1 << 1,
    String = 1 << 2,
    ByteArray = 1 << 3,
    VariableReference = 1 << 4,
    
    Any = 0xFF,
}

