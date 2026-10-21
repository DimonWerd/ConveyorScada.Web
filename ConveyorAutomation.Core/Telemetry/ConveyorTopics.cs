namespace ConveyorAutomation.Telemetry;

/// <summary>
/// MQTT-теми конвеєра. Єдине місце, де вони описані: ними користуються
/// і симулятор (публікація), і SCADA-вебзастосунок (мнемосхема).
/// </summary>
public static class ConveyorTopics
{
    public const string Base = "conveyor/1";

    public const string InputCargo       = Base + "/sensors/input/cargo";
    public const string BeltSpeed        = Base + "/sensors/belt/speed";
    public const string MotorTemperature = Base + "/sensors/motor/temperature";
    public const string MotorCurrent     = Base + "/sensors/motor/current";

    public const string State = Base + "/state";

    /// <summary>Фільтр для підписки на всі теми конвеєра.</summary>
    public const string Filter = Base + "/#";
}
