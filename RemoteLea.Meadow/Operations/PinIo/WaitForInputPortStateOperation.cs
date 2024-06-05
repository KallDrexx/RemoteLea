using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.PinIo;

public class WaitForInputPortStateOperation : OperationBase
{
    public const string OpCodeValue = "wait_for_input_state";
    public const string PortVariableParam = "PortVariable";
    public const string StateToWaitFor = "DesiredState";
    
    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(PortVariableParam, ParameterType.VariableReference,
            "Variable containing the input port"),
        
        new OperationParameter(StateToWaitFor, ParameterType.Bool, "The state to wait for"),
    };

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

        context.Log(LogLevel.Info, $"Waiting for digital input port '{portVariable.Value.VariableName} to reach state '{desiredState}'");
        while (true)
        {
            await Task.Delay(1);
            bool state;
            if (port is IDigitalInputPort digitalInputPort)
            {
                state = digitalInputPort.State;
            }
            else if (port is IDigitalInterruptPort digitalInterruptPort)
            {
                state = digitalInterruptPort.State;
            }
            else
            {
                context.Log(LogLevel.Error, $"The specified port variable '{portVariable.Value.VariableName}' " +
                                            "is not a digital input or interrupt port");
                return OperationExecutionResult.Failure();
            }
            
            if (state == desiredState.Value)
            {
                break;
            }
        }
        
        context.Log(LogLevel.Info, $"Digital input port '{portVariable.Value.VariableName} reached state '{desiredState}'");
        return OperationExecutionResult.Success();
    }
}