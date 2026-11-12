using System;
using NUnit.Framework;
using Handler = TickHandler.UnityTickHandler.UnityTickHandler;

namespace TickHandler.Tests
{
public sealed class UnityTickHandlerTests
{
	private const float Step = 0.5f;

	private FakeDispatcher _dispatcher;
	private Handler _tickHandler;

	[SetUp]
	public void SetUp()
	{
		_dispatcher = new FakeDispatcher();
		_tickHandler = new Handler(_dispatcher);
	}

	[TearDown]
	public void TearDown()
	{
		_tickHandler.Dispose();
	}

	[Test]
	public void FrameUpdate_ListenerUnsubscribingItself_DoesNotThrowAndStopsReceiving()
	{
		var calls = 0;
		Action<float> listener = null;
		listener = _ =>
		{
			calls++;
			_tickHandler.UnsubscribeOnFrameUpdate(listener);
		};
		_tickHandler.SubscribeOnFrameUpdate(listener);

		Assert.DoesNotThrow(() => _dispatcher.RaiseUpdate());
		_dispatcher.RaiseUpdate();

		Assert.AreEqual(1, calls);
	}

	[Test]
	public void FrameUpdate_ListenerUnsubscribingLaterListener_SkipsItInTheSamePass()
	{
		var laterCalls = 0;
		Action<float> later = _ => laterCalls++;
		_tickHandler.SubscribeOnFrameUpdate(_ => _tickHandler.UnsubscribeOnFrameUpdate(later));
		_tickHandler.SubscribeOnFrameUpdate(later);

		_dispatcher.RaiseUpdate();
		_dispatcher.RaiseUpdate();

		Assert.AreEqual(0, laterCalls);
	}

	[Test]
	public void FrameUpdate_ListenerSubscribingAnother_StartsItOnTheNextPass()
	{
		var addedCalls = 0;
		var subscribed = false;
		_tickHandler.SubscribeOnFrameUpdate(_ =>
		{
			if (subscribed)
			{
				return;
			}

			subscribed = true;
			_tickHandler.SubscribeOnFrameUpdate(__ => addedCalls++);
		});

		_dispatcher.RaiseUpdate();
		Assert.AreEqual(0, addedCalls, "A listener added during a pass should wait for the next one");

		_dispatcher.RaiseUpdate();
		Assert.AreEqual(1, addedCalls);
	}

	[Test]
	public void FrameUpdate_RemainingListenersKeepTheirOrderAfterRemovalDuringPass()
	{
		var order = string.Empty;
		Action<float> second = _ => order += "2";
		_tickHandler.SubscribeOnFrameUpdate(_ =>
		{
			order += "1";
			_tickHandler.UnsubscribeOnFrameUpdate(second);
		});
		_tickHandler.SubscribeOnFrameUpdate(second);
		_tickHandler.SubscribeOnFrameUpdate(_ => order += "3");

		_dispatcher.RaiseUpdate();
		_dispatcher.RaiseUpdate();

		Assert.AreEqual("1313", order);
	}

	[Test]
	public void OtherPhases_ListenerUnsubscribingItself_DoesNotThrow()
	{
		Action<float> late = null;
		late = _ => _tickHandler.UnsubscribeOnFrameLateUpdate(late);
		Action<float> physics = null;
		physics = _ => _tickHandler.UnsubscribeOnPhysicUpdate(physics);
		Action<float> endFrame = null;
		endFrame = _ => _tickHandler.UnsubscribeOnEndFrameUpdate(endFrame);
		_tickHandler.SubscribeOnFrameLateUpdate(late);
		_tickHandler.SubscribeOnPhysicUpdate(physics);
		_tickHandler.SubscribeOnEndFrameUpdate(endFrame);

		Assert.DoesNotThrow(() => _dispatcher.RaiseLateUpdate());
		Assert.DoesNotThrow(() => _dispatcher.RaiseFixedUpdate());
		Assert.DoesNotThrow(() => _dispatcher.RaiseEndFrameUpdate());
	}

	[Test]
	public void LateUpdateOnce_ListenerRunsOnce()
	{
		var calls = 0;
		_tickHandler.SubscribeOnLateUpdateOnce(_ => calls++);

		_dispatcher.RaiseLateUpdate();
		_dispatcher.RaiseLateUpdate();

		Assert.AreEqual(1, calls);
	}

	[Test]
	public void LateUpdateOnce_ListenerSubscribingAnotherOnce_RunsItOnTheNextLateUpdate()
	{
		var chainedCalls = 0;
		_tickHandler.SubscribeOnLateUpdateOnce(_ => _tickHandler.SubscribeOnLateUpdateOnce(__ => chainedCalls++));

		Assert.DoesNotThrow(() => _dispatcher.RaiseLateUpdate());
		Assert.AreEqual(0, chainedCalls);

		_dispatcher.RaiseLateUpdate();
		_dispatcher.RaiseLateUpdate();
		Assert.AreEqual(1, chainedCalls);
	}

	[Test]
	public void Dispose_FromInsideListener_StopsTheRemainingListeners()
	{
		var laterCalls = 0;
		_tickHandler.SubscribeOnFrameUpdate(_ => _tickHandler.Dispose());
		_tickHandler.SubscribeOnFrameUpdate(_ => laterCalls++);

		Assert.DoesNotThrow(() => _dispatcher.RaiseUpdate());
		_dispatcher.RaiseUpdate();

		Assert.AreEqual(0, laterCalls);
	}

	private sealed class FakeDispatcher : IDispatcher
	{
		public event Action<float> OnUpdate;
		public event Action<float> OnLateUpdate;
		public event Action<float> OnFixedUpdate;
		public event Action<float> OnEndFrameUpdate;

		public float DeltaTime => Step;

		public void RaiseUpdate() => OnUpdate?.Invoke(Step);

		public void RaiseLateUpdate() => OnLateUpdate?.Invoke(Step);

		public void RaiseFixedUpdate() => OnFixedUpdate?.Invoke(Step);

		public void RaiseEndFrameUpdate() => OnEndFrameUpdate?.Invoke(Step);

		public void Dispose()
		{
		}
	}
}
}
