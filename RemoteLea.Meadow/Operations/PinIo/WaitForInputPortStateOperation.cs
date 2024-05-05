using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.PinIo;

[Operation(OpCode)]
[OperationParameter(0, PortVariableParam, ParameterType.VariableReference, "Variable containing the input port")]
[OperationParameter(1, StateToWaitFor, ParameterType.Bool, "The state to wait for")]
public class WaitForInputPortStateOperation : OperationBase
{
    public const string OpCode = "wait_for_input_state";
    public const string PortVariableParam = "PortVariable";
    public const string StateToWaitFor = "DesiredState";
    
    protected override async ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var portVariable = context.ParseVariableArgument(PortVariableParam);
        if (portVariable == null)
        {
            context.LogInvalidRequiredArgument(PortVariableParam, ParameterType.VariableReference);
            return OperationExecutionResult.Failure();
        }
        
        if (context.Variables.TryGetValue(portVariable.Value.VariableName, out var port) == false)
        {
            context.Log(LogLevel.Error, $"The specified port variable '{portVariable.Value.VariableName}' does not exist");
            return OperationExecutionResult.Failure();
        }
        
        var desiredState = context.ParseBoolArgument(StateToWaitFor);
        if (desiredState == null)
        {
            context.LogInvalidRequiredArgument(StateToWaitFor, ParameterType.Bool);
            return OperationExecutionResult.Failure();
        }
        
        if (port is not IDigitalInputPort digitalInputPort)
        {
            context.Log(LogLevel.Error, $"The specified port variable '{portVariable.Value.VariableName}' is not a digital input port");
            return OperationExecutionResult.Failure();
        }
        
        context.Log(LogLevel.Debug, $"Waiting for digital input port '{portVariable.Value.VariableName} to reach state '{desiredState}'");
        while (digitalInputPort.State != desiredState.Value)
        {
            await Task.Delay(100);
        }
        
        context.Log(LogLevel.Debug, $"Digital input port '{portVariable.Value.VariableName} reached state '{desiredState}'");
        return OperationExecutionResult.Success();
    }
}