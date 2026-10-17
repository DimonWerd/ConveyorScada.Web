namespace ConveyorAutomation;

/// <summary>Датчик струму приводу: струм зростає зі швидкістю стрічки.</summary>
public class MotorCurrentSensor : Sensor
{
    private readonly ConveyorMotor _motor;
    private readonly Random _random = new();

    public MotorCurrentSensor(string name, ConveyorMotor motor) : base(name, "A")
    {
        _motor = motor;
    }

    public override double Read()
    {
        if (_motor.CurrentSpeed <= 0)
        {
            LastValue = 0;
            return LastValue;
        }

        double load = _motor.CurrentSpeed / ConveyorMotor.MaxSpeed;
        double noise = (_random.NextDouble() - 0.5) * 0.4;
        LastValue = Math.Max(0, 2.0 + 8.0 * load + noise);
        return LastValue;
    }
}
