using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.I2c;

public class InitI2CBus : OperationBase
{
    public const string OpCodeValue = "init_i2c_bus";
    public const string StorageVariable = "StorageVariable";
    private readonly II2cController _i2CController;
    
    protected override string OpCode => OpCodeValue;

    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(StorageVariable, ParameterType.VariableReference, "Variable to store the i2c bus in"),
    };

    public InitI2CBus(II2cController i2CController)
    {
        _i2CController = i2CController;
    }

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var variable = context.ParseVariableArgument(StorageVariable);
        if (variable == null)
        {
            context.LogInvalidRequiredArgument(StorageVariable, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        var bus = _i2CController.CreateI2cBus();
        context.Outputs[variable.Value.VariableName] = bus;

        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}