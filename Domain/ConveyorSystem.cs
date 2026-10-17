namespace ConveyorAutomation;

/// <summary>Система керування конвеєром: об'єднує сенсори та привід і реалізує логіку керування.</summary>
public class ConveyorSystem
{
    private const int IdleTicksToStop = 3;       // кроків без вантажу до зупинки

    private int _idleTicks;

    public ConveyorMotor Motor { get; }
    public CargoSensor CargoSensor { get; }
    public SpeedSensor SpeedSensor { get; }
    public MotorTemperatureSensor TemperatureSensor { get; }
    public MotorCurrentSensor CurrentSensor { get; }

    public double NominalSpeed { get; set; } = 1.0;       // м/с
    public double OverspeedLimit { get; set; } = 1.5;     // м/с
    public string State { get; private set; } = "Зупинено";

    public ConveyorSystem(
        ConveyorMotor motor,
        CargoSensor cargoSensor,
        SpeedSensor speedSensor,
        MotorTemperatureSensor temperatureSensor,
        MotorCurrentSensor currentSensor)
    {
        Motor = motor;
        CargoSensor = cargoSensor;
        SpeedSensor = speedSensor;
        TemperatureSensor = temperatureSensor;
        CurrentSensor = currentSensor;
    }

    /// <summary>Один цикл керування: опитування сенсорів і прийняття рішення.</summary>
    public void Update()
    {
        Motor.Update();

        // Кожен сенсор опитується рівно один раз за цикл;
        // далі всі (зокрема MQTT-публікатор) користуються LastValue.
        double cargo = CargoSensor.Read();
        double speed = SpeedSensor.Read();
        TemperatureSensor.Read();
        CurrentSensor.Read();

        if (Motor.IsEmergencyStopped)
        {
            State = "АВАРІЯ";
            return;
        }

        // Захист: перевищення допустимої швидкості
        if (speed > OverspeedLimit)
        {
            Motor.EmergencyStop();
            State = "АВАРІЯ: перевищення швидкості";
            return;
        }

        if (cargo > 0)
        {
            _idleTicks = 0;
            if (!Motor.IsRunning)
                Motor.Start(NominalSpeed);
        }
        else
        {
            _idleTicks++;
            if (_idleTicks >= IdleTicksToStop && Motor.IsRunning)
                Motor.Stop();
        }

        State = Motor.IsRunning ? "Працює" : "Зупинено";
    }

    /// <summary>Аварійна зупинка за командою оператора.</summary>
    public void EmergencyStop()
    {
        Motor.EmergencyStop();
        State = "АВАРІЯ: зупинка оператором";
    }

    public void ResetEmergency()
    {
        Motor.ResetEmergency();
        _idleTicks = 0;
        State = "Зупинено";
    }
}
