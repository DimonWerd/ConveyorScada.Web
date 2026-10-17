namespace ConveyorAutomation;

/// <summary>Датчик швидкості руху стрічки (з невеликим шумом вимірювання).</summary>
public class SpeedSensor : Sensor
{
    private readonly ConveyorMotor _motor;
    private readonly Random _random = new();

    public SpeedSensor(string name, ConveyorMotor motor) : base(name, "м/с")
    {
        _motor = motor;
    }

    public override double Read()
    {
        double noise = _motor.CurrentSpeed > 0 ? (_random.NextDouble() - 0.5) * 0.04 : 0;
        LastValue = Math.Max(0, _motor.CurrentSpeed + noise);
        return LastValue;
    }
}
