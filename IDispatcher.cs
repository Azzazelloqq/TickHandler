using System;

namespace TickHandler
{
public interface IDispatcher : IDisposable
{
    public event Action<float> OnUpdate;
    public event Action<float> OnLateUpdate;
    public event Action<float> OnFixedUpdate;
    public event Action<float> OnEndFrameUpdate;
}
}