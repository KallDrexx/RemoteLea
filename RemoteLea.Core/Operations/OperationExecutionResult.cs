namespace RemoteLea.Core.Operations;

public readonly struct OperationExecutionResult
{
    public readonly bool WasSuccessful;
    public readonly string? JumpToLabel;

    private OperationExecutionResult(bool success, string? jumpToLabel)
    {
        WasSuccessful = success;
        JumpToLabel = jumpToLabel;
    }

    public static OperationExecutionResult Failure()
    {
        return new OperationExecutionResult(false, null);
    }

    public static OperationExecutionResult Success(string? jumpToLabel = null)
    {
        return new OperationExecutionResult(true, jumpToLabel);
    }
}