namespace BaconEggs;

public sealed class EventListeners
{
    private readonly InputState _keys;
    private readonly Func<long> _timeProvider;

    public EventListeners(InputState keys, Func<long> timeProvider)
    {
        _keys = keys;
        _timeProvider = timeProvider;
    }

    public long LastTime { get; private set; }

    public void HandleKeyDown(string key)
    {
        switch (key)
        {
            case "w": _keys.W.Pressed = true; break;
            case "a": _keys.A.Pressed = true; break;
            case "s": _keys.S.Pressed = true; break;
            case "d": _keys.D.Pressed = true; break;
        }
    }

    public void HandleKeyUp(string key)
    {
        switch (key)
        {
            case "w": _keys.W.Pressed = false; break;
            case "a": _keys.A.Pressed = false; break;
            case "s": _keys.S.Pressed = false; break;
            case "d": _keys.D.Pressed = false; break;
        }
    }

    public void HandleVisibilityChanged(bool isHidden)
    {
        if (!isHidden)
        {
            LastTime = _timeProvider();
        }
    }
}
