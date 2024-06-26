﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Meadow.Hardware;
using RemoteLea.Core;
using RemoteLea.Core.Operations;

namespace RemoteLea.Meadow.Operations.Pwm;

public class SetPwmDutyCycleOperation : OperationBase
{
    public const string OpCodeValue = "set_pwm_duty_cycle";
    
    private const string VariableParam = "Variable";
    private const string DutyCycleParam = "DutyCyclePercent";
    
    protected override string OpCode => OpCodeValue;
    
    protected override IReadOnlyList<OperationParameter> Parameters => new[]
    {
        new OperationParameter(VariableParam, ParameterType.VariableReference, 
            "Variable with the pwm port"),
        
        new OperationParameter(DutyCycleParam, ParameterType.Integer,
            "Percent between 0 and 100 for the duty cycle of the pwm"),
    };

    protected override ValueTask<OperationExecutionResult> ExecuteInternalAsync(IOperationExecutionContext context)
    {
        var variable = context.ParseVariableArgument(VariableParam);
        if (variable == null)
        {
            context.LogInvalidRequiredArgument(VariableParam, ParameterType.VariableReference);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        var dutyCycle = context.ParseIntArgument(DutyCycleParam);
        if (dutyCycle == null)
        {
            context.LogInvalidRequiredArgument(DutyCycleParam, ParameterType.Integer);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }
        
        if (!context.Variables.TryGetValue(variable.Value.VariableName, out var variableValue))
        {
            var message = $"No known variable with the name '{variable.Value.VariableName}'";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (variableValue is not IPwmPort pwmPort)
        {
            var message = $"Variable '{variable.Value.VariableName}' is a {variable.GetType().Name} " +
                          $"but an IPwmPort was expected.";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        if (dutyCycle is < 0 or > 100)
        {
            var message = $"Duty cycle percent should be between 0 and 100, but instead was {dutyCycle}";
            context.Log(LogLevel.Error, message);
            return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Failure());
        }

        pwmPort.DutyCycle = dutyCycle.Value / 100f;
        return new ValueTask<OperationExecutionResult>(OperationExecutionResult.Success());
    }
}