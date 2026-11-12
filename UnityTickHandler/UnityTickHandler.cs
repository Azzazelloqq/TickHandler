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
	private readonly ListenerList _updateListeners;
	private readonly ListenerList _lateUpdateListeners;
	private readonly ListenerList _endFrameUpdateListeners;
	private readonly ListenerList _physicsListeners;
	private readonly List<Action<float>> _lateUpdateOnceListeners;

	// The once-listeners being invoked in the current LateUpdate; reused to avoid a per-frame allocation.
	private readonly List<Action<float>> _lateUpdateOnceBatch;

	/// <summary>
	/// Initializes a new instance of the <see cref="UnityTickHandler"/> class.
	/// </summary>
	/// <param name="dispatcher">The dispatcher to subscribe to.</param>
	/// <param name="listenersCapacity">The initial capacity for listener lists.</param>
	public UnityTickHandler(IDispatcher dispatcher, int listenersCapacity = 50)
	{
		_dispatcher = dispatcher;
		_updateListeners = new ListenerList(listenersCapacity);
		_lateUpdateListeners = new ListenerList(listenersCapacity);
		_physicsListeners = new ListenerList(listenersCapacity);
		_lateUpdateOnceListeners = new List<Action<float>>(listenersCapacity);
		_lateUpdateOnceBatch = new List<Action<float>>(listenersCapacity);
		_endFrameUpdateListeners = new ListenerList(listenersCapacity);

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
		_lateUpdateOnceBatch.Clear();
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
		if (listener == null)
		{
			return;
		}

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
		_physicsListeners.Invoke(deltaTime);
	}

	private void OnDispatcherUpdateFrame(float deltaTime)
	{
		FrameUpdate?.Invoke(deltaTime);
		_updateListeners.Invoke(deltaTime);
	}

	private void OnDispatcherLateUpdateFrame(float deltaTime)
	{
		FrameLateUpdate?.Invoke(deltaTime);
		_lateUpdateListeners.Invoke(deltaTime);
		InvokeLateUpdateOnceListeners(deltaTime);
	}

	private void OnDispatcherEndFrameUpdate(float deltaTime)
	{
		EndFrameUpdate?.Invoke(deltaTime);
		_endFrameUpdateListeners.Invoke(deltaTime);
	}

	private void InvokeLateUpdateOnceListeners(float deltaTime)
	{
		if (_lateUpdateOnceListeners.Count == 0)
		{
			return;
		}

		// The batch is moved out before it runs, so a once-listener that subscribes another one
		// schedules it for the next LateUpdate instead of changing the list being walked.
		_lateUpdateOnceBatch.AddRange(_lateUpdateOnceListeners);
		_lateUpdateOnceListeners.Clear();

		try
		{
			// Count is read on every step: Dispose called from a listener empties the batch.
			for (var i = 0; i < _lateUpdateOnceBatch.Count; i++)
			{
				_lateUpdateOnceBatch[i].Invoke(deltaTime);
			}
		}
		finally
		{
			_lateUpdateOnceBatch.Clear();
		}
	}

	/// <summary>
	/// A listener list that can be changed from inside a listener. A listener removed during a pass is not
	/// invoked later in that pass; a listener added during a pass is first invoked on the next one.
	/// </summary>
	private sealed class ListenerList
	{
		private static readonly Predicate<Action<float>> IsRemovedSlot = listener => listener == null;

		private readonly List<Action<float>> _listeners;
		private int _activePasses;
		private bool _hasRemovedSlots;

		public ListenerList(int capacity)
		{
			_listeners = new List<Action<float>>(capacity);
		}

		public void Add(Action<float> listener)
		{
			if (listener == null)
			{
				return;
			}

			_listeners.Add(listener);
		}

		public void Remove(Action<float> listener)
		{
			if (listener == null)
			{
				return;
			}

			var index = _listeners.IndexOf(listener);
			if (index < 0)
			{
				return;
			}

			if (_activePasses == 0)
			{
				_listeners.RemoveAt(index);
				return;
			}

			// A pass is walking the list by index: blank the slot now and compact once the pass ends.
			_listeners[index] = null;
			_hasRemovedSlots = true;
		}

		public void Clear()
		{
			if (_activePasses == 0)
			{
				_listeners.Clear();
				return;
			}

			for (var i = 0; i < _listeners.Count; i++)
			{
				_listeners[i] = null;
			}

			_hasRemovedSlots = true;
		}

		public void Invoke(float deltaTime)
		{
			// Listeners added during this pass land past this count and wait for the next pass.
			var count = _listeners.Count;

			_activePasses++;
			try
			{
				for (var i = 0; i < count; i++)
				{
					_listeners[i]?.Invoke(deltaTime);
				}
			}
			finally
			{
				_activePasses--;
				if (_activePasses == 0 && _hasRemovedSlots)
				{
					_listeners.RemoveAll(IsRemovedSlot);
					_hasRemovedSlots = false;
				}
			}
		}
	}
}
}