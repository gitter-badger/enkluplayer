### Overview

EnkluPlayer comes with a small tweening system made specifically for tweening element properties. While it could be extended to tween anything, the focus on element properties means all the guarantees of the prop system may be extended to the tweening system.

A simple JS interface is included for completeness, but not discussed in much detail here.

##### TweenData

Tweens use a POCO to define their behavior. This means that tweens can be easily serialized, duplicated, or passed around as messages. The `TweenData` object itself is a grab bag of required and optional parameters that are used by `Tween` objects. The C# tweening system does not provide much in the way of nice patterns for building `TweenData` objects, though the JS API does.

Look at the `TweenData` object for a full reference of all supported options.

##### Tween

The `Tween` object is given a schema to act on and the `TweenData` instance that provides all the data necessary for tweening.

```csharp
var a = new Tween(element.Schema, new TweenData { Prop = "Alpha", To = 1, DurationSec = 5f });
```

Unlike the usual `Update(dt)` interfaces found in many libraries, `Tween` simply has a `float T` that can be set to any value. This means that `Tween` instances may be paused, played in reverse, or even scrubbed with no instability. Subclasses tell the `Tween` object how to calculate these values.

```csharp
var a = new Tween(schema, data);
a.T = 0.1f;

// no step size or implicit calculation, everything is explicit
a.T += 5f;
```

##### Easing Equations

All the easing equations are taken from Robert Penner's easing equations ([Terms of Use](http://www.robertpenner.com/easing_terms_of_use.html)) and documented as such in source. Options include:

```
Linear
Bounce
Exponential
Quadratic
Cubic
Quartic
Quintic
```

These are specified in the `TweenData`:

```csharp
new TweenData
{
    Easing = TweenEasingTypes.CubicInOut
}
```

These equations are separate enough to be useful outside of the tween system as well.

##### TweenManager

This object is responsible for creating, tracking, and updating `Tween` instances, though this is just for ease of use. Any object may create or advance a `Tween`.

```csharp
var tweens = new TweenManager();
var a = tweens.Float(myElement, new TweenData { ... });

// elided

void Enter()
{
    tweens.Start(a);
}

void Update()
{
    tweens.Update(Time.dt);
}
```

Tweens are started, stopped, paused, or resumed from this interface.

```csharp
tweens.Start(a);		// a::T == 0
tweens.Update(0.5f); 	// a::T == 0.5f
tweens.Pause(a);

tweens.Update(20f);	// a::T == 0.5f
tweens.Resume(a);
tweens.Update(20f);	// a::T == 20.5f

```

When `Stop()` has been called for a `Tween`, `Start` is the only way to start it again, which will set the time to 0.

##### Extending Tween

The tweening system supports several types: `float`, `vec3`, and `col4`. To add support for more, simply subclass `Tween` and override the `Update` method. Here is a full example that adds support for a `UnityEngine.Vector3`:

```csharp
/// <summary>
/// Tween for a Vector3.
/// </summary>
public class Vector3Tween : Tween
{
    /// <summary>
    /// The prop.
    /// </summary>
    private readonly ElementSchemaProp<Vector3> _prop;

    /// <summary>
    /// The original value of the prop.
    /// </summary>
    private readonly Vector3 _originalValue;

    /// <summary>
    /// The difference between start and ending values.
    /// </summary>
    private readonly Vector3 _diff;

    /// <summary>
    /// Constructor.
    /// </summary>
    public Vec3Tween(ElementSchema schema, TweenData data)
        : base(schema, data)
    {
        _prop = schema.Get<Vector3>(Data.Prop);
        _originalValue = data.CustomFrom ? (Vector3) data.From : _prop.Value;
        _diff = (Vector3) data.To - _originalValue;
    }

    /// <inheritdoc />
    protected override void Update(float parameter)
    {
        _prop.Value = _originalValue + parameter * _diff;
    }
}
```

The creation of tweens is _baked in_ to `TweenManager`-- there is no separate factory that creates `Tween` instances from data. Perhaps this will be something we do in the future, but for now it's been judged more useful to have the factory built-in to the `TweenManager` interface.

##### JS API

The JS API is specified via `TweenManagerJsApi.cs` and `TweenJs.cs`. These two objects roughly mirror the C# interface, but provide a nice builder pattern. to create tweens. More can be found on the JS documentation pages.