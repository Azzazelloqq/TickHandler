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
	/// <summary>
	/// Occurs every frame during the Update phase.
	/// </summary>
	public event Action<float> OnUpdate;

	/// <summary>
	/// Occurs every frame during the LateUpdate phase.
	/// </summary>
	public event Action<float> OnLateUpdate;

	/// <summary>
	/// Occurs every physics frame during the FixedUpdate phase.
	/// </summary>
	public event Action<float> OnFixedUpdate;

	/// <summary>
	/// Occurs at the end of the LateUpdate phase.
	/// </summary>
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