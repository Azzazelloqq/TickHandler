using System;

namespace TickHandler
{
/// <summary>
/// Interface for dispatching frame events in Unity.
/// </summary>
public interface IDispatcher : IDisposable
{
	/// <summary>
	/// Event that occurs every frame during the Update phase.
	/// </summary>
	event Action<float> OnUpdate;

	/// <summary>
	/// Event that occurs every frame during the LateUpdate phase.
	/// </summary>
	event Action<float> OnLateUpdate;

	/// <summary>
	/// Event that occurs every physics frame during the FixedUpdate phase.
	/// </summary>
	event Action<float> OnFixedUpdate;

	/// <summary>
	/// Event that occurs at the end of the LateUpdate phase.
	/// </summary>
	event Action<float> OnEndFrameUpdate;
}
}