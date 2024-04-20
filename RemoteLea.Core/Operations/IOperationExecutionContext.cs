using System.Collections.Generic;
using System.Threading;

namespace RemoteLea.Core.Operations;

/// <summary>
/// All information needed for an operation to execute
/// </summary>
public interface IOperationExecutionContext
{
    IReadOnlyDictionary<string, IArgumentValue> Arguments { get; }
    IReadOnlyDictionary<string, object> Variables { get; }
    Dictionary<string, object> Outputs { get; }
    InstructionLogFunction Log { get; }
    CancellationToken CancellationToken { get; }
}
