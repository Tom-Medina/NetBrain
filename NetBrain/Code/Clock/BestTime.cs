namespace NetBrain.Code.Clock;

public enum Region
{
    EU,
    US
}

public record ScheduleSlot(string Platform, Region Region, TimeOnly Time);

public static class BestTime
{
    private static readonly List<ScheduleSlot> Timeline = new List<ScheduleSlot>
    {
        new("TikTok", Region.US, new TimeOnly(2, 0)),
        new("Instagram", Region.US, new TimeOnly(3, 0)),
        new("X", Region.EU, new TimeOnly(9, 0)),
        new("Reddit", Region.US, new TimeOnly(14, 30)),
        new("X", Region.US, new TimeOnly(15, 0)),
        new("YouTube", Region.EU, new TimeOnly(17, 0)),
        new("TikTok", Region.EU, new TimeOnly(20, 0)),
        new("Instagram", Region.EU, new TimeOnly(21, 0)),
        new("YouTube", Region.US, new TimeOnly(22, 0))
    };

    public static List<ScheduleSlot> GetTimeline() => Timeline;
}
