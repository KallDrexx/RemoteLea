using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.PinIo;

public class SaveInputStateOperation : OperationBase
{
    public const string OpCodeValue = "save_input_state";
    public const string PortVariableParam = "PortVariable";
    public const string StateVariableParam = "StateVariable";
    
    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(PortVariableParam, ParameterType.VariableReference,
            "Variable containing the port to save the state of"),
        
        new OperationParameter(StateVariableParam, ParameterType.VariableReference, "Variable to store the state in"),
    };

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var portVariable = context.ParseVariableArgument(PortVariableParam);
        if (portVariable == null)
        {
            context.LogInvalidRequiredArgument(PortVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (context.Variables.TryGetValue(portVariable.Value.VariableName, out var port) == false)
        {
            context.Log(LogLevel.Error, $"The specified port variable '{portVariable.Value.VariableName}' does not exist");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var stateVariable = context.ParseVariableArgument(StateVariableParam);
        if (stateVariable == null)
        {
            context.LogInvalidRequiredArgument(StateVariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (port is not IDigitalInputPort digitalInputPort)
        {
            context.Log(LogLevel.Error, $"The specified port variable '{portVariable.Value.VariableName}' is not a digital input port");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        context.Outputs[stateVariable.Value.VariableName] = digitalInputPort.State;
        context.Log(LogLevel.Debug, $"Saved state of digital input port '{portVariable.Value.VariableName} to variable '{stateVariable.Value.VariableName}'");
        
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}