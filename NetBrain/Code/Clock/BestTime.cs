namespace NetBrain.Code.Clock;

public enum Region
{
    EU,
    US,
    RU
}

public record ScheduleSlot(string Platform, Region Region, TimeOnly Time);

public static class BestTime
{
    private static readonly List<ScheduleSlot> Timeline = new List<ScheduleSlot>
    {
        new("tiktok", Region.US, new TimeOnly(2, 0)),
        new("instagram", Region.US, new TimeOnly(3, 0)),
        new("x", Region.EU, new TimeOnly(9, 0)),
        new("reddit", Region.US, new TimeOnly(14, 30)),
        new("x", Region.US, new TimeOnly(15, 0)),
        new("youtube", Region.EU, new TimeOnly(17, 0)),
        new("vk", Region.RU, new TimeOnly(18, 0)),
        new("tiktok", Region.EU, new TimeOnly(20, 0)),
        new("instagram", Region.EU, new TimeOnly(21, 0)),
        new("youtube", Region.US, new TimeOnly(22, 0))
    };

    public static List<ScheduleSlot> GetTimeline() => Timeline;
}
