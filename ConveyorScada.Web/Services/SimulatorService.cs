using ConveyorAutomation.Devices;
using ConveyorAutomation.Sensors;
using ConveyorAutomation.Systems;
using ConveyorAutomation.Mqtt;

namespace ConveyorScada.Web.Services
{
    /// <summary>
    /// Симулятор конвеєра: періодично опитує сенсори (модель з ЛР3)
    /// і публікує показання та стан у MQTT-брокер.
    /// </summary>
    public class SimulatorService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SimulatorService> _logger;

        public SimulatorService(IConfiguration config, ILogger<SimulatorService> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string host = _config["Mqtt:Host"]!;
            int port = int.Parse(_config["Mqtt:Port"]!);
            string baseTopic = _config["Simulator:BaseTopic"]!;
            int cycles = int.Parse(_config["Simulator:Cycles"] ?? "0");      // 0 = працювати безперервно
            int delayMs = int.Parse(_config["Simulator:DelayMs"] ?? "2000");

            // ---------- Модель із лабораторної роботи 3 ----------
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
                new SensorMqttConfig("input-cargo",       system.CargoSensor,       $"{baseTopic}/sensors/input/cargo",        "bool"),
                new SensorMqttConfig("belt-speed",        system.SpeedSensor,       $"{baseTopic}/sensors/belt/speed",         "m/s"),
                new SensorMqttConfig("motor-temperature", system.TemperatureSensor, $"{baseTopic}/sensors/motor/temperature", "C"),
                new SensorMqttConfig("motor-current",     system.CurrentSensor,     $"{baseTopic}/sensors/motor/current",      "A"),
            };
            string stateTopic = $"{baseTopic}/state";

            await using var publisher = new MqttPublisher(host, port);

            for (int cycle = 1; (cycles == 0 || cycle <= cycles) && !stoppingToken.IsCancellationRequested; cycle++)
            {
                system.Update();   // опитує всі сенсори й виконує логіку керування

                try
                {
                    // якщо брокер недоступний (або з'єднання втрачено) - підключаємось знову на наступному циклі
                    if (!publisher.IsConnected)
                    {
                        await publisher.ConnectAsync(stoppingToken);
                        _logger.LogInformation("Симулятор підключено до MQTT-брокера {Host}:{Port}", host, port);
                    }

                    foreach (var cfg in sensorConfigs)
                    {
                        string json = await publisher.PublishReadingAsync(cfg, stoppingToken);
                        _logger.LogInformation("{Topic} -> {Json}", cfg.Topic, json);
                    }

                    var state = new ConveyorStateMessage(
                        system.State,
                        motor.IsRunning,
                        motor.IsEmergencyStopped,
                        system.CargoSensor.TotalCargoCount,
                        DateTime.UtcNow);

                    // retain: новий підписник одразу отримає останній стан
                    string stateJson = await publisher.PublishJsonAsync(stateTopic, state, retain: true, stoppingToken);
                    _logger.LogInformation("{Topic} -> {Json}", stateTopic, stateJson);

                    await Task.Delay(delayMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Не вдалося опублікувати дані ({Message}), повтор через 5 с", ex.Message);
                    try { await Task.Delay(5000, stoppingToken); }
                    catch (OperationCanceledException) { break; }
                }
            }

            _logger.LogInformation("Симуляцію завершено");
        }
    }
}
