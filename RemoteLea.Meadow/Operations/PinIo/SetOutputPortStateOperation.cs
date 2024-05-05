using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.PinIo;

public class SetOutputPortStateOperation : OperationBase
{
    private const string PortNameParameterName = "port_name";
    private const string StateParameterName = "state";
    
    protected override string OpCode => "set_output_state";

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(PortNameParameterName, ParameterType.VariableReference,
            "Variable holding the digital output port"),
        
        new OperationParameter(StateParameterName, ParameterType.Bool,
            "State to set the port to (true = high, false = low)"),
    };
    
    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var portVariable = context.ParseVariableArgument(PortNameParameterName);
        if (portVariable == null)
        {
            context.LogInvalidRequiredArgument(PortNameParameterName, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var state = context.ParseBoolArgument(StateParameterName);
        if (state == null)
        {
            context.LogInvalidRequiredArgument(StateParameterName, ParameterType.Bool);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (context.Variables.TryGetValue(portVariable.Value.VariableName, out var port) == false)
        {
            context.Log(LogLevel.Error, $"The specified port variable '{portVariable.Value.VariableName}' does not exist");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (port is not IDigitalOutputPort digitalOutputPort)
        {
            context.Log(LogLevel.Error, $"The specified port variable '{portVariable.Value.VariableName}' is not a digital output port");
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        digitalOutputPort.State = state.Value;
        context.Log(LogLevel.Debug, $"Set state of digital output port '{portVariable.Value.VariableName}' to '{state.Value}'");
        
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}