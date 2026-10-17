namespace ConveyorAutomation.Mqtt;

// Назви властивостей (у camelCase) мають збігатися з полями, які очікує веб-застосунок:
// value, unit, measuredAtUtc.

/// <summary>JSON-повідомлення з показанням сенсора.</summary>
public record SensorReadingMessage(string Sensor, double Value, string Unit, DateTime MeasuredAtUtc);

/// <summary>JSON-повідомлення зі станом конвеєра.</summary>
public record ConveyorStateMessage(string State, bool MotorRunning, bool Emergency, int TotalCargo, DateTime MeasuredAtUtc);
