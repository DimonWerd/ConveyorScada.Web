using ConveyorAutomation.Devices;

namespace ConveyorAutomation.Sensors;

/// <summary>Датчик температури приводу: нагрівається під навантаженням і остигає після зупинки.</summary>
public class MotorTemperatureSensor : Sensor
{
    private const double Ambient = 25.0;         // температура навколишнього середовища, C
    private const double MaxRise = 25.0;         // нагрів на максимальній швидкості, C

    private readonly ConveyorMotor _motor;
    private readonly Random _random = new();
    private double _temperature = Ambient;

    public MotorTemperatureSensor(string name, ConveyorMotor motor) : base(name, "C")
    {
        _motor = motor;
    }

    public override double Read()
    {
        double target = Ambient + MaxRise * (_motor.CurrentSpeed / ConveyorMotor.MaxSpeed);
        _temperature += (target - _temperature) * 0.25;   // інерційність нагріву
        LastValue = _temperature + (_random.NextDouble() - 0.5) * 0.3;
        return LastValue;
    }
}
