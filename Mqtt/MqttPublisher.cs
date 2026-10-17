using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Protocol;

namespace ConveyorAutomation.Mqtt;

/// <summary>Обгортка над MQTTnet 5: підключення до брокера та публікація JSON-повідомлень.</summary>
public class MqttPublisher : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IMqttClient _client;
    private readonly MqttClientOptions _options;

    public MqttPublisher(string host = "127.0.0.1", int port = 1883, string clientId = "conveyor-simulator")
    {
        _client = new MqttClientFactory().CreateMqttClient();
        _options = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithClientId(clientId)
            .WithCleanSession()
            .Build();
    }

    public bool IsConnected => _client.IsConnected;

    /// <summary>Підключення до брокера.</summary>
    public Task ConnectAsync(CancellationToken ct = default) => _client.ConnectAsync(_options, ct);

    /// <summary>Публікує останнє показання сенсора (LastValue) у його тему.</summary>
    public Task<string> PublishReadingAsync(SensorMqttConfig config, CancellationToken ct = default)
    {
        var message = new SensorReadingMessage(
            config.SensorId,
            Math.Round(config.Sensor.LastValue, 2),
            config.Unit,
            DateTime.UtcNow);

        return PublishJsonAsync(config.Topic, message, retain: false, ct);
    }

    /// <summary>Публікує довільний об'єкт у форматі JSON. Повертає надісланий JSON.</summary>
    public async Task<string> PublishJsonAsync<T>(string topic, T payload, bool retain, CancellationToken ct = default)
    {
        string json = JsonSerializer.Serialize(payload, JsonOptions);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(json))
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(retain)
            .Build();

        await _client.PublishAsync(message, ct);
        return json;
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsConnected)
        {
            await _client.DisconnectAsync(new MqttClientDisconnectOptions
            {
                Reason = MqttClientDisconnectOptionsReason.NormalDisconnection
            });
        }

        _client.Dispose();
    }
}
