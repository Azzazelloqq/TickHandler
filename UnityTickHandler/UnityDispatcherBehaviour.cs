using System;
using UnityEngine;

namespace TickHandler.UnityTickHandler
{
public class UnityDispatcherBehaviour : MonoBehaviour, IDispatcher
{
    public event Action<float> OnUpdate;
    public event Action<float> OnLateUpdate;
    public event Action<float> OnFixedUpdate;
    public event Action<float> OnEndFrameUpdate;

    private void Update()
    {
        OnUpdate?.Invoke(Time.deltaTime);
    }

    private void LateUpdate()
    {
        OnLateUpdate?.Invoke(Time.deltaTime);
        OnEndFrameUpdate?.Invoke(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        OnFixedUpdate?.Invoke(Time.fixedDeltaTime);
    }

    public void Dispose()
    {
        OnUpdate = null;
        OnLateUpdate = null;
        OnFixedUpdate = null;
    }
}
}