using System;

namespace TickHandler
{
public interface ITickHandler : IDisposable
{
    public event Action<float> FrameUpdate;
    public event Action<float> FrameLateUpdate;
    public event Action<float> EndFrameUpdate; 
    public event Action<float> PhysicUpdate;
    
    internal IDispatcher Dispatcher { get; }
    
    public void SubscribeOnFrameUpdate(Action<float> listener);
    public void SubscribeOnFrameLateUpdate(Action<float> listener);
    public void SubscribeOnPhysicUpdate(Action<float> listener);
    public void UnsubscribeOnFrameUpdate(Action<float> listener);
    public void UnsubscribeOnFrameLateUpdate(Action<float> listener);
    public void UnsubscribeOnPhysicUpdate(Action<float> listener);
    public void SubscribeOnLateUpdateOnce(Action<float> listener);
    public void SubscribeOnEndFrameUpdate(Action<float> listener);
    public void UnsubscribeOnEndFrameUpdate(Action<float> listener);

}
}