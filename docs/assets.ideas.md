##### Initializers

Initializers are objects that preprocess assets every time they are loaded. This could be useful if, for instance, a specific `MonoBehaviour` should be added to each asset with a specific naming convention.

```csharp
// register intitializer
foreach (var reference in _assets.Find(Tags.Runnable))
{
	// async method that will always be executed before load comes back
    reference.Use((asset, next) => {
        asset
            .Run()
            .OnFinally(_ => next());
    });  
}

...

// elsewhere in the codebase
var reference = _assets.Get(guid);
reference
    .Load<MyThing>()
    .OnSuccess(thing =>
    {
        // async thing.Run() has already been called by initializer!
        
        thing.Foo();
    });
```