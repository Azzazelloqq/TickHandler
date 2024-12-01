using System;
using System.Collections.Generic;

namespace TickHandler.UnityTickHandler
{
/// <summary>
/// Handles tick events by subscribing to an <see cref="IDispatcher"/> and managing listeners.
/// </summary>
public class UnityTickHandler : ITickHandler
{
	/// <inheritdoc/>
	public event Action<float> FrameUpdate;

	/// <inheritdoc/>
	public event Action<float> FrameLateUpdate;

	/// <inheritdoc/>
	public event Action<float> EndFrameUpdate;

	/// <inheritdoc/>
	public event Action<float> PhysicUpdate;
	
	/// <inheritdoc/>
	public float DeltaTime => _dispatcher.DeltaTime;

	IDispatcher ITickHandler.Dispatcher => _dispatcher;

	private readonly IDispatcher _dispatcher;
	private readonly List<Action<float>> _updateListeners;
	private readonly List<Action<float>> _lateUpdateListeners;
	private readonly List<Action<float>> _endFrameUpdateListeners;
	private readonly List<Action<float>> _physicsListeners;
	private readonly List<Action<float>> _lateUpdateOnceListeners;

	/// <summary>
	/// Initializes a new instance of the <see cref="UnityTickHandler"/> class.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to subscribe to.</param>
	/// <param name="listenersCapacity">The initial capacity for listener lists.</param>
	public UnityTickHandler(IDispatcher dispatcher, int listenersCapacity = 50)
	{
		_dispatcher = dispatcher;
		_updateListeners = new List<Action<float>>(listenersCapacity);
		_lateUpdateListeners = new List<Action<float>>(listenersCapacity);
		_physicsListeners = new List<Action<float>>(listenersCapacity);
		_lateUpdateOnceListeners = new List<Action<float>>(listenersCapacity);
		_endFrameUpdateListeners = new List<Action<float>>(listenersCapacity);

		SubscribeOnDispatcherEvents();
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		UnsubscribeOnDispatcherEvents();

		_updateListeners.Clear();
		_lateUpdateListeners.Clear();
		_physicsListeners.Clear();
		_lateUpdateOnceListeners.Clear();
		_endFrameUpdateListeners.Clear();
	}

	/// <inheritdoc/>
	public void SubscribeOnFrameUpdate(Action<float> listener)
	{
		_updateListeners.Add(listener);
	}

	/// <inheritdoc/>
	public void SubscribeOnFrameLateUpdate(Action<float> listener)
	{
		_lateUpdateListeners.Add(listener);
	}

	/// <inheritdoc/>
	public void SubscribeOnPhysicUpdate(Action<float> listener)
	{
		_physicsListeners.Add(listener);
	}

	/// <inheritdoc/>
	public void UnsubscribeOnFrameUpdate(Action<float> listener)
	{
		_updateListeners.Remove(listener);
	}

	/// <inheritdoc/>
	public void UnsubscribeOnFrameLateUpdate(Action<float> listener)
	{
		_lateUpdateListeners.Remove(listener);
	}

	/// <inheritdoc/>
	public void UnsubscribeOnPhysicUpdate(Action<float> listener)
	{
		_physicsListeners.Remove(listener);
	}

	/// <inheritdoc/>
	public void SubscribeOnLateUpdateOnce(Action<float> listener)
	{
		_lateUpdateOnceListeners.Add(listener);
	}

	/// <inheritdoc/>
	public void SubscribeOnEndFrameUpdate(Action<float> listener)
	{
		_endFrameUpdateListeners.Add(listener);
	}

	/// <inheritdoc/>
	public void UnsubscribeOnEndFrameUpdate(Action<float> listener)
	{
		_endFrameUpdateListeners.Remove(listener);
	}

	private void SubscribeOnDispatcherEvents()
	{
		_dispatcher.OnUpdate += OnDispatcherUpdateFrame;
		_dispatcher.OnLateUpdate += OnDispatcherLateUpdateFrame;
		_dispatcher.OnFixedUpdate += OnDispatcherPhysicsFrame;
		_dispatcher.OnEndFrameUpdate += OnDispatcherEndFrameUpdate;
	}

	private void UnsubscribeOnDispatcherEvents()
	{
		_dispatcher.OnUpdate -= OnDispatcherUpdateFrame;
		_dispatcher.OnLateUpdate -= OnDispatcherLateUpdateFrame;
		_dispatcher.OnFixedUpdate -= OnDispatcherPhysicsFrame;
		_dispatcher.OnEndFrameUpdate -= OnDispatcherEndFrameUpdate;
	}

	private void OnDispatcherPhysicsFrame(float deltaTime)
	{
		PhysicUpdate?.Invoke(deltaTime);
		foreach (var physicsListener in _physicsListeners)
		{
			physicsListener?.Invoke(deltaTime);
		}
	}

	private void OnDispatcherUpdateFrame(float deltaTime)
	{
		FrameUpdate?.Invoke(deltaTime);
		foreach (var updateListener in _updateListeners)
		{
			updateListener?.Invoke(deltaTime);
		}
	}

	private void OnDispatcherLateUpdateFrame(float deltaTime)
	{
		FrameLateUpdate?.Invoke(deltaTime);
		foreach (var lateUpdateListener in _lateUpdateListeners)
		{
			lateUpdateListener?.Invoke(deltaTime);
		}

		foreach (var lateUpdateOnceListener in _lateUpdateOnceListeners)
		{
			lateUpdateOnceListener?.Invoke(deltaTime);
		}

		_lateUpdateOnceListeners.Clear();
	}

	private void OnDispatcherEndFrameUpdate(float deltaTime)
	{
		EndFrameUpdate?.Invoke(deltaTime);

		foreach (var endFrameUpdateListener in _endFrameUpdateListeners)
		{
			endFrameUpdateListener?.Invoke(deltaTime);
		}
	}
}
}