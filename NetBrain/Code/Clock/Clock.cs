using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetBrain.Code.Clock;

public class Clock
{
    private readonly List<IClockListener> _listeners = new();
    private readonly TimeSpan _tickInterval;
    private CancellationTokenSource _cts;

    public Clock(TimeSpan tickInterval)
    {
        _tickInterval = tickInterval;
    }

    public void AddListener(IClockListener listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        Task.Run(() => TickLoop(_cts.Token));
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private async Task TickLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var now = DateTime.Now;
            foreach (var listener in _listeners)
            {
                try
                {
                    listener.OnTick(now);
                }
                catch
                {
                    // Ne jamais laisser une exception bloquer le loop
                }
            }

            await Task.Delay(_tickInterval, token);
        }
    }
}