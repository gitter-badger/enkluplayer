### Overview

`IMetricsService`  describes a set of methods and objects that allow for simple data collection.

#### IMetricsTarget

Any metrics an `IMetricsService` captures are sent to an `IMetricsTarget`. The target can make decisions about aggregating data and forwarding it to a metrics backend. A target is created and added to the service:

```csharp
_metrics.AddTarget(new HostedGraphiteMetricsTarget(hostname, appKey));
```

#### Timers

A timer may be created using the `Timer` method. This method requires a key to associate with the timer. 

```csharp
var timer = _metrics.Timer("app.data.load");
```

The `TimerMetric` object that is retrieved will associate whatever timers you start and stop with that key. A measurement may be taken with `Start` and `Stop`. 

```csharp
var id = timer.Start();

...
    
timer.Stop(id);
```

`Start` returns a unique id for that measurement. This id is then used to stop the timer.

This object does not represent a *single* timer, but an object that can time process associated with that key. That is, each time a `TimerMetric` is requested for a key, _the same `TimerMetric` will be retrieved_. This metric object may be cached and used many times.

```csharp
var a = timer.Start();

...

var b = timer.Start();

...

timer.Stop(a);

...
 
timer.Stop(b);
```

A timer may also be aborted. This is handy if an asynchronous process ended in an error and the resulting time should be discarded.

```csharp
var id = timer.Start();
...
timer.Abort(id);
```

#### Counter

For an integral metric, use `Counter`. This will retrieve a `CounterMetric`.

```csharp
var counter = _metrics.Counter("user.logins");

// value = value + 1
counter.Increment();

// value = value - 1
counter.Decrement();

// value += 12
counter.Add(12);

// value -= 12
counter.Subtract(12);

// value = 17
counter.Count(17);
```

