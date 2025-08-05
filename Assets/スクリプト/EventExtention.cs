using System;
public class EventHandlerDisposable
{
    public event EventHandler Event;

    public void Invoke()
    {
        Event?.Invoke(this, EventArgs.Empty);
        Event = null;
    }
    public static EventHandlerDisposable operator +(EventHandlerDisposable wrapper, EventHandler handler)
    {
        if (wrapper == null || wrapper.Event == null) wrapper = new();
        wrapper.Event += handler;
        return wrapper;
    }

    public static EventHandlerDisposable operator -(EventHandlerDisposable wrapper, EventHandler handler)
    {
        wrapper.Event -= handler;
        return wrapper;
    }
}