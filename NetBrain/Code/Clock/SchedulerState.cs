namespace NetBrain.Code.Clock;

public class SchedulerState
{
    public string LastDate { get; set; } = "";
    public List<string> PostedToday { get; set; } = new();
    public Dictionary<string, int> DailyOffsets { get; set; } = new();
}
