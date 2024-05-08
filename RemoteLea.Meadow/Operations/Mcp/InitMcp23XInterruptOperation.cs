using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Mcp;

public class InitMcp23XInterruptOperation : OperationBase
{
    private const string OpCodeValue = "init_mcp23x_interrupt";
    private const string McpVariableParam = "McpVariable";
    private const string PinNameParam = "PinName";
    private const string StorageVariableParam = "Variable";

    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(McpVariableParam, ParameterType.VariableReference,
            "Variable containing the mcp instance"),
        
        new OperationParameter(PinNameParam, ParameterType.String, "Name of the mcp pin"),
        new OperationParameter(StorageVariableParam, ParameterType.VariableReference, "Variable to store the port in"),
    };
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var mcpVariableName = context.ParseVariableArgument(McpVariableParam);
        if (mcpVariableName == null)
        {
            context.LogInvalidRequiredArgument(McpVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (context.Variables.TryGetValue(mcpVariableName.Value.VariableName, out var mcpObject) == false)
        {
            context.Log(LogLevel.Error, $"The specified mcp variable '{mcpVariableName.Value.VariableName}' does not exist");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (mcpObject is not Mcp23xxx mcp)
        {
            context.Log(LogLevel.Error, $"The specified mcp variable '{mcpVariableName.Value.VariableName}' is not an MCP23008 instance");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var storageVariableName = context.ParseVariableArgument(StorageVariableParam);
        if (storageVariableName == null)
        {
            context.LogInvalidRequiredArgument(StorageVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var pinName = context.ParseStringArgument(PinNameParam);
        if (pinName == null)
        {
            context.LogInvalidRequiredArgument(PinNameParam, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var pin = mcp.GetPin(pinName);
        if (pin == null)
        {
            context.Log(LogLevel.Error, $"The specified MCP pin '{pinName}' does not exist.");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var interrupt = pin.CreateDigitalInterruptPort(InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp);
        context.Outputs[storageVariableName.Value.VariableName] = interrupt;
        
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}