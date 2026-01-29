namespace NetBrain.Code.Clock;

public interface IClockListener
{
    void OnTick(DateTime now);
}