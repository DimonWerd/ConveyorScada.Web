namespace Models.Entities
{
    /// <summary>Стан конвеєра (тема conveyor/1/state).</summary>
    public class ConveyorState
    {
        public string State { get; set; } = "";
        public bool MotorRunning { get; set; }
        public bool Emergency { get; set; }
        public int TotalCargo { get; set; }
        public DateTimeOffset MeasuredAtUtc { get; set; }
    }
}
