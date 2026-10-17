namespace ConveyorAutomation;

/// <summary>Датчик наявності вантажу на вхідній ділянці стрічки.</summary>
public class CargoSensor : Sensor
{
    private readonly Random _random = new();
    private readonly double _probability;

    public bool CargoDetected { get; private set; }
    public int TotalCargoCount { get; private set; }

    /// <param name="probability">Імовірність появи вантажу під датчиком (0..1).</param>
    public CargoSensor(string name, double probability = 0.4) : base(name, "")
    {
        _probability = Math.Clamp(probability, 0, 1);
    }

    public override double Read()
    {
        bool detected = _random.NextDouble() < _probability;

        // Новий вантаж рахуємо лише в момент появи (фронт сигналу)
        if (detected && !CargoDetected)
            TotalCargoCount++;

        CargoDetected = detected;
        LastValue = detected ? 1 : 0;
        return LastValue;
    }

    public override string ToString() =>
        $"{Name}: {(CargoDetected ? "вантаж є" : "вантажу немає")} (всього: {TotalCargoCount})";
}
