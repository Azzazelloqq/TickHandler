using System;

namespace TickHandler
{
/// <summary>
/// Interface for handling tick events and managing listeners.
/// </summary>
public interface ITickHandler : IDisposable
{
	/// <summary>
	/// Event that occurs every frame during the Update phase.
	/// </summary>
	event Action<float> FrameUpdate;

	/// <summary>
	/// Event that occurs every frame during the LateUpdate phase.
	/// </summary>
	event Action<float> FrameLateUpdate;

	/// <summary>
	/// Event that occurs at the end of the LateUpdate phase.
	/// </summary>
	event Action<float> EndFrameUpdate;

	/// <summary>
	/// Event that occurs every physics frame during the FixedUpdate phase.
	/// </summary>
	event Action<float> PhysicUpdate;

	/// <summary>
	/// Internal dispatcher used by the tick handler.
	/// </summary>
	internal IDispatcher Dispatcher { get; }

	/// <summary>
	/// Subscribes a listener to the FrameUpdate event.
	/// </summary>
	/// <param name="listener">The listener to subscribe.</param>
	void SubscribeOnFrameUpdate(Action<float> listener);

	/// <summary>
	/// Subscribes a listener to the FrameLateUpdate event.
	/// </summary>
	/// <param name="listener">The listener to subscribe.</param>
	void SubscribeOnFrameLateUpdate(Action<float> listener);

	/// <summary>
	/// Subscribes a listener to the PhysicUpdate event.
	/// </summary>
	/// <param name="listener">The listener to subscribe.</param>
	void SubscribeOnPhysicUpdate(Action<float> listener);

	/// <summary>
	/// Unsubscribes a listener from the FrameUpdate event.
	/// </summary>
	/// <param name="listener">The listener to unsubscribe.</param>
	void UnsubscribeOnFrameUpdate(Action<float> listener);

	/// <summary>
	/// Unsubscribes a listener from the FrameLateUpdate event.
	/// </summary>
	/// <param name="listener">The listener to unsubscribe.</param>
	void UnsubscribeOnFrameLateUpdate(Action<float> listener);

	/// <summary>
	/// Unsubscribes a listener from the PhysicUpdate event.
	/// </summary>
	/// <param name="listener">The listener to unsubscribe.</param>
	void UnsubscribeOnPhysicUpdate(Action<float> listener);

	/// <summary>
	/// Subscribes a listener to the LateUpdate event for one-time invocation.
	/// </summary>
	/// <param name="listener">The listener to subscribe.</param>
	void SubscribeOnLateUpdateOnce(Action<float> listener);

	/// <summary>
	/// Subscribes a listener to the EndFrameUpdate event.
	/// </summary>
	/// <param name="listener">The listener to subscribe.</param>
	void SubscribeOnEndFrameUpdate(Action<float> listener);

	/// <summary>
	/// Unsubscribes a listener from the EndFrameUpdate event.
	/// </summary>
	/// <param name="listener">The listener to unsubscribe.</param>
	void UnsubscribeOnEndFrameUpdate(Action<float> listener);
}
}