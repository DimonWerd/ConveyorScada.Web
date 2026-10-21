using ConveyorAutomation.Devices;
using ConveyorAutomation.Sensors;
using ConveyorAutomation.Systems;
using ConveyorAutomation.Telemetry;
using ConveyorScada.Simulator.Mqtt;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ---------- Налаштування ----------
const string BrokerHost = "127.0.0.1";
const int BrokerPort = 1883;
const int DelayMs = 2000;                                  // пауза між циклами, мс
int cycles = args.Length > 0 ? int.Parse(args[0]) : 0;     // 0 = працювати до Ctrl+C (dotnet run -- 10)

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;   // даємо програмі завершитись коректно
    cts.Cancel();
};
CancellationToken ct = cts.Token;

// ---------- Модель з бібліотеки ConveyorAutomation.Core ----------
var motor = new ConveyorMotor("Привід стрічки");
var system = new ConveyorSystem(
    motor,
    new CargoSensor("Датчик вантажу", 0.5),
    new SpeedSensor("Датчик швидкості", motor),
    new MotorTemperatureSensor("Температура приводу", motor),
    new MotorCurrentSensor("Струм приводу", motor));

// ---------- Сенсор -> MQTT-тема -> одиниця вимірювання ----------
var sensorConfigs = new[]
{
    new SensorMqttConfig("input-cargo",       system.CargoSensor,       ConveyorTopics.InputCargo,       "bool"),
    new SensorMqttConfig("belt-speed",        system.SpeedSensor,       ConveyorTopics.BeltSpeed,        "m/s"),
    new SensorMqttConfig("motor-temperature", system.TemperatureSensor, ConveyorTopics.MotorTemperature, "C"),
    new SensorMqttConfig("motor-current",     system.CurrentSensor,     ConveyorTopics.MotorCurrent,     "A"),
};

await using var publisher = new MqttPublisher(BrokerHost, BrokerPort);
Console.WriteLine($"Симулятор конвеєра. Брокер: {BrokerHost}:{BrokerPort}. Зупинка: Ctrl+C");

// ---------- Цикл симулятора ----------
for (int cycle = 1; (cycles == 0 || cycle <= cycles) && !ct.IsCancellationRequested; cycle++)
{
    system.Update();   // опитує всі сенсори й виконує логіку керування

    try
    {
        // якщо брокер недоступний або з'єднання втрачено - підключаємось знову
        if (!publisher.IsConnected)
        {
            await publisher.ConnectAsync(ct);
            Console.WriteLine("Підключено до MQTT-брокера");
        }

        Console.WriteLine();
        Console.WriteLine($"--- Цикл {cycle}: {system.State} ---");

        foreach (var config in sensorConfigs)
        {
            string json = await publisher.PublishReadingAsync(config, ct);
            Console.WriteLine($"{config.Topic} -> {json}");
        }

        var state = new ConveyorStateMessage(
            system.State,
            motor.IsRunning,
            motor.IsEmergencyStopped,
            system.CargoSensor.TotalCargoCount,
            DateTime.UtcNow);

        // retain: новий підписник одразу отримає останній стан
        string stateJson = await publisher.PublishJsonAsync(ConveyorTopics.State, state, retain: true, ct);
        Console.WriteLine($"{ConveyorTopics.State} -> {stateJson}");

        await Task.Delay(DelayMs, ct);
    }
    catch (OperationCanceledException)
    {
        break;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Брокер недоступний або помилка публікації ({ex.Message}), повтор через 5 с");
        try { await Task.Delay(5000, ct); }
        catch (OperationCanceledException) { break; }
    }
}

Console.WriteLine("Симулятор зупинено.");
