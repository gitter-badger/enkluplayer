### Overview

The `ApplicationStateService` is an [`ApplicationService`](application.md) that wraps a simple finite state machine. Each state represents some major piece of application logic.

##### States

The interface for a finite state machine can be very simple. At the time of this writing, the full interface for our `IState` is below:

```csharp
/// <summary>
/// Basic interface for a state.
/// </summary>
public interface IState
{
    /// <summary>
    /// Called when the state is transitioned to.
    /// </summary>
    /// <param name="context">Optionally passed in to state.</param>
    void Enter(object context);

    /// <summary>
    /// Called every frame.
    /// </summary>
    /// <param name="dt">Delta time.</param>
    void Update(float dt);

    /// <summary>
    /// Called when the state is transitioned out.
    /// </summary>
    void Exit();
}
```

An FSM then, is equally simple, exposing a single `ChangeState<T>` method that switches between states. All major states of the application are implemented using this simple interface. Our states also do not contain logic about what states to switch into. They simply dispatch events about what they are doing.

##### Flows

Because `EnkluPlayer` targets many different platforms, we have introduced an object called a *flow*. A flow is an object that determines *how and when* states transition into each other. On HoloLens, for instance, we want to move from `Login` directly into `Play`. On Mobile devices, we want to move from `Login` to `UserProfile`. On Web, we don't use `Login` at all. Thus, there is not a good place to put any of that logic at the state level. We could move up the chain and bake this into the FSM, in this case the `ApplicationStateService`, but this would get very messy. Instead, we bake this logic into each of the different [flow implementations](https://github.com/enklu/enkluplayer/tree/master/Assets/Source/Application/Flows): one per platform.

This gives us an extra benefit: we can easily test out a specific target application flow *without actually using that build target*.

