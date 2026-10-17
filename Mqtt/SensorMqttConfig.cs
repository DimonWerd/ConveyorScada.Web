namespace ConveyorAutomation.Mqtt;

/// <summary>Зв'язує об'єкт сенсора з MQTT-темою та одиницею вимірювання.</summary>
/// <param name="SensorId">Короткий ідентифікатор сенсора (потрапляє в JSON).</param>
/// <param name="Sensor">Об'єкт сенсора з лабораторної роботи 3.</param>
/// <param name="Topic">MQTT-тема, в яку публікуються показання.</param>
/// <param name="Unit">Одиниця вимірювання (потрапляє в JSON).</param>
public record SensorMqttConfig(string SensorId, Sensor Sensor, string Topic, string Unit);
