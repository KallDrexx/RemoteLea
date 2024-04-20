namespace RemoteLea.Core;

/// <summary>
/// A value that is provided for an instruction's argument
/// </summary>
public interface IArgumentValue;

/// <summary>
/// An integer value for an argument
/// </summary>
public readonly record struct IntArgumentValue(int Value) : IArgumentValue;

/// <summary>
/// A string value for an argument
/// </summary>
public readonly record struct StringArgumentValue(string Value) : IArgumentValue;

/// <summary>
/// A boolean value for an argument
/// </summary>
public readonly record struct BoolArgumentValue(bool Value) : IArgumentValue;

/// <summary>
/// A byte array value for an argument
/// </summary>
public readonly record struct ByteArrayArgumentValue(byte[] Value) : IArgumentValue;

/// <summary>
/// An argument value that points refers to a variable with the given name.
/// </summary>
public readonly record struct VariableReferenceArgumentValue(string VariableName) : IArgumentValue;
