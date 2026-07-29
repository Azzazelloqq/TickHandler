# Tick Handler for Unity

> Explicit subscriptions to Unity frame phases without spreading `Update` methods through the project.

`TickHandler` adapts Unity callbacks into a disposable `ITickHandler`. It is
useful for services and plain C# objects that need frame notifications while
keeping Unity-specific code in one dispatcher component.

## Features

- `Update`, `LateUpdate`, `FixedUpdate` and end-of-frame notifications.
- Delta time passed to every listener.
- One-time late-update subscriptions.
- Disposable subscriptions and handler lifetime.
- Plain C# `UnityTickHandler` over a scene `UnityDispatcherBehaviour`.

## Installation

```bash
git submodule add https://github.com/Azzazelloqq/TickHandler.git Assets/TickHandler
```

Or add to `Packages/manifest.json`:

```json
"com.azzazello.tickhandler": "https://github.com/Azzazelloqq/TickHandler.git"
```

## Setup

1. Add `UnityDispatcherBehaviour` to a GameObject in the scene.
2. Create `UnityTickHandler` with that dispatcher.
3. Subscribe application services to the required phase.

```csharp
using TickHandler.UnityTickHandler;
using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    [SerializeField] private UnityDispatcherBehaviour _dispatcher;

    private UnityTickHandler _ticks;

    private void Awake()
    {
        _ticks = new UnityTickHandler(_dispatcher);
        _ticks.SubscribeOnFrameUpdate(OnFrameUpdate);
        _ticks.SubscribeOnLateUpdateOnce(OnFirstLateUpdate);
    }

    private void OnDestroy()
    {
        _ticks.Dispose();
    }

    private void OnFrameUpdate(float deltaTime)
    {
    }

    private void OnFirstLateUpdate(float deltaTime)
    {
    }
}
```

## Phases

| `ITickHandler` member | Unity source |
| --- | --- |
| `FrameUpdate` | `Update` |
| `FrameLateUpdate` | `LateUpdate` |
| `PhysicUpdate` | `FixedUpdate` |
| `EndFrameUpdate` | after `LateUpdate` listeners |

`SubscribeOnLateUpdateOnce` invokes a listener during the next late update and
removes it automatically.
