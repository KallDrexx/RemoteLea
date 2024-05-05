using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Foundation.ICs.IOExpanders;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Mcp;

public class InitMcp23XOutputOperation : OperationBase
{
    public const string OpCodeValue = "init_mcp23x_output";
    private const string McpVariableParam = "McpVariable";
    private const string PinNameParam = "PinName";
    private const string StorageVariableParam = "Variable";

    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(McpVariableParam, ParameterType.VariableReference,
            "Variable containing the mcp instance"),
        new OperationParameter(PinNameParam, ParameterType.String, "Name of the pin"),
        new OperationParameter(StorageVariableParam, ParameterType.VariableReference, "Variable to store the port in"),
    };

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var mcpVariable = context.ParseVariableArgument(McpVariableParam);
        if (mcpVariable == null)
        {
            context.LogInvalidRequiredArgument(McpVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (context.Variables.TryGetValue(mcpVariable.Value.VariableName, out var mcpObject) == false)
        {
            context.Log(LogLevel.Error,
                $"The specified mcp variable '{mcpVariable.Value.VariableName}' does not exist");
            
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (mcpObject is not Mcp23xxx mcp)
        {
            context.Log(LogLevel.Error,
                $"The specified mcp variable '{mcpVariable.Value.VariableName}' is not an MCP23008 instance");
            
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var pinName = context.ParseStringArgument(PinNameParam);
        if (pinName == null)
        {
            context.LogInvalidRequiredArgument(PinNameParam, ParameterType.String);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var pin = mcp.GetPin(pinName);
        if (pin == null) // can be true despite the `GetPin` method missing the `?`
        {
            context.Log(LogLevel.Error, $"The specified pin '{pinName}' does not exist on this device");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var variable = context.ParseVariableArgument(StorageVariableParam);
        if (variable == null)
        {
            context.LogInvalidRequiredArgument(StorageVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var port = mcp.CreateDigitalOutputPort(pin);
        context.Outputs[variable.Value.VariableName] = port;

        context.Log(LogLevel.Debug,
            $"Digital output port created for mcp pin ${pinName} to variable '${variable.Value.VariableName}'");

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}