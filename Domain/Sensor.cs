namespace ConveyorAutomation;

/// <summary>Базовий клас для всіх сенсорів конвеєра.</summary>
public abstract class Sensor
{
    public string Name { get; }
    public string Unit { get; }
    public double LastValue { get; protected set; }

    protected Sensor(string name, string unit)
    {
        Name = name;
        Unit = unit;
    }

    /// <summary>Виконує вимірювання та повертає значення.</summary>
    public abstract double Read();

    public override string ToString() => $"{Name}: {LastValue:F2} {Unit}".TrimEnd();
}
