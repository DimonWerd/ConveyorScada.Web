namespace ConveyorAutomation;

/// <summary>Привід (електродвигун) стрічки конвеєра.</summary>
public class ConveyorMotor
{
    public const double MaxSpeed = 2.0;          // м/с
    private const double Acceleration = 0.5;     // зміна швидкості за один крок, м/с

    public string Name { get; }
    public bool IsRunning { get; private set; }
    public bool IsEmergencyStopped { get; private set; }
    public double TargetSpeed { get; private set; }
    public double CurrentSpeed { get; private set; }

    public ConveyorMotor(string name)
    {
        Name = name;
    }

    /// <summary>Запускає привід із заданою швидкістю.</summary>
    public void Start(double speed)
    {
        if (IsEmergencyStopped) return;          // після аварії потрібне скидання
        TargetSpeed = Math.Clamp(speed, 0, MaxSpeed);
        IsRunning = TargetSpeed > 0;
    }

    /// <summary>Штатна зупинка (плавне гальмування).</summary>
    public void Stop()
    {
        IsRunning = false;
        TargetSpeed = 0;
    }

    /// <summary>Аварійна зупинка (миттєва).</summary>
    public void EmergencyStop()
    {
        IsRunning = false;
        IsEmergencyStopped = true;
        TargetSpeed = 0;
        CurrentSpeed = 0;
    }

    /// <summary>Скидання аварійного стану оператором.</summary>
    public void ResetEmergency() => IsEmergencyStopped = false;

    /// <summary>Один крок моделювання: швидкість плавно прямує до заданої.</summary>
    public void Update()
    {
        if (CurrentSpeed < TargetSpeed)
            CurrentSpeed = Math.Min(CurrentSpeed + Acceleration, TargetSpeed);
        else if (CurrentSpeed > TargetSpeed)
            CurrentSpeed = Math.Max(CurrentSpeed - Acceleration, TargetSpeed);
    }

    public override string ToString() =>
        $"{Name}: {(IsEmergencyStopped ? "АВАРІЯ" : IsRunning ? "працює" : "зупинений")}, " +
        $"швидкість {CurrentSpeed:F2}/{TargetSpeed:F2} м/с";
}
