using System;
using UnityEngine;

namespace TickHandler.UnityTickHandler
{
/// <summary>
/// A Unity MonoBehaviour that dispatches Unity lifecycle events to subscribed listeners.
/// Implements the <see cref="IDispatcher"/> interface.
/// </summary>
public class UnityDispatcherBehaviour : MonoBehaviour, IDispatcher
{
	/// <inheritdoc/>
	public event Action<float> OnUpdate;

	/// <inheritdoc/>
	public event Action<float> OnLateUpdate;

	/// <inheritdoc/>
	public event Action<float> OnFixedUpdate;

	/// <inheritdoc/>
	public event Action<float> OnEndFrameUpdate;

	/// <inheritdoc/>
	public float DeltaTime => Time.deltaTime;

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

	/// <summary>
	/// Disposes the dispatcher by unsubscribing all listeners.
	/// </summary>
	public void Dispose()
	{
		OnUpdate = null;
		OnLateUpdate = null;
		OnFixedUpdate = null;
		OnEndFrameUpdate = null;
	}
}
}