using System.Collections.Concurrent;
using Models.Entities;

namespace ConveyorScada.Web.Services
{
    /// <summary>Потокобезпечний кеш останніх значень (за MQTT-темою).</summary>
    public class LiveValuesCache
    {
        private readonly ConcurrentDictionary<string, SensorReading> _values = new();
        private ConveyorState? _state;

        public void Update(SensorReading reading) => _values[reading.Topic] = reading;

        public void UpdateState(ConveyorState state) => _state = state;

        public IReadOnlyDictionary<string, SensorReading> GetAll() => _values;

        public ConveyorState? State => _state;
    }
}
