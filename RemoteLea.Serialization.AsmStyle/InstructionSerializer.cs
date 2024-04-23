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
    
}