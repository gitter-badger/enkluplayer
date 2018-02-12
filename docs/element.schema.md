### Overview

The [Element](element.md) system provides a simple node graph implementation, much like the [DOM](https://developer.mozilla.org/en-US/docs/Web/API/Document_Object_Model/Introduction) or [Cocos](http://www.cocos2d-x.org/) (or pretty much every other game engine under the sun). The intention of the element system is to provide a basis for many types of composite structures, which is a helpful mechanism for managing scenes. The need also arises for elements to have a composite state structure, which is what the _Schema_ system provides.

### Web Tech

If we look to web technology, there exists a method for decorating elements with a type of state-- style state, and that is CSS. What's handy about CSS is that it allows you to share and build on this state in a variety of ways that don't quite mimic object oriented constructs. Unfortunately, CSS is limited to style data, but it provides a clear way of applying attributes to various elements and reusing these attributes across groups of elements.

### Schema

Every `Element` has a `Schema` property which may store any object by a key. Similar to a dictionary, but values are wrapped in arguments that fire events and propagate changes.

```csharp
var prop = schema.Get<int>("foo");
print(prop.Value); // 0

prop.Value = 5;
print(prop.Value); // 5
```

This gets or creates a new `ElementSchemaProp<int>` object that is managed by `schema`. We are guaranteed that a call to `Get<T>` will _never_ return null. In the case above, `default(int)` is the starting value for the prop. This object may be cached and polled or its event may be used to listen for updates.

```csharp
var prop = schema.Get<int>("foo");
prop.OnChanged += (p, prev, next) => {
  print(string.Format(
  	"{0} changed from {1} -> {2}",
  	p,
  	prev,
  	next));
};

prop.Value = 5; // foo changed from 0 -> 5
```

The `Set` method may be used when we don't wish to get a value first.

```csharp
// equivalent
prop.Set("foo", 12);
prop.Get<int>("foo").Value = 12;
```

This is interesting, at the very least, because we can put anything in here, not just primitives.

```csharp
prop.Set("asset", new Asset());
```

But it gets more interesting.

### Inheritance

When an element is a child of another, its `Schema` is also a child of its parent. If a property is request from a `Schema` and it doesn't have it, before creating a new one, it will move up the graph and see if one can be found.

```csharp
var parent = new Element();
parent.Schema.Set("foo", 17);

var child = new Element();
parent.AddChild(child);

print(child.Get<int>("foo").Value); // 17
```

In this example, child did not have a property "foo", so it looked to the parent. The parent in turn checks its own list of properties and if it doesn't have it, looks to its parent-- so the nearest ancestor property will be used.

What happens if you want to change the value of a property that was taken from a parent? In this case, the link is broken and a new property is created on the child with the new value.

```csharp
var parentProp = parent.Schema.Get<int>("foo");
var childProp = child.Schema.Get<int>("foo");

print(parentProp.Value); // 17
print(childProp.Value); // 17

parentProp.Value = 31;

print(parentProp.Value); // 31
print(childProp.Value); // 31

childProp.Value = 4;

print(parentProp.Value); // 31
print(childProp.Value); // 4
```

### Miscellany

There are a few other methods that bear mentioning.

##### HasProp

This simply returns true if and only if a property is already defined in the current schema or parent schemas.

```csharp
if (schema.HasProp("myProp")) {
  //
}
```

##### HasOwnProp

This method returns true if and only if a property is already defined in the current schema. This method does not look to parent schemas..

```csharp
if (!schema.HasOwnProp("myProp")) {
  // using ancestor property
}
```

### Edge Cases

There are also some undesirable edge cases. The most egregious is probably requesting a property with the matching name but incorrect type. In this case, the first to declare the prop wins. The later prop is a dead prop which cannot be changed from the default value of the type.

```csharp

var intProp = schema.Get<int>("foo");
var stringProp = schema.Get<string>("foo");

intProp.Value = 5;
print(intProp.Value); // 5

stringProp.Value = "Hello World";
print(stringProp.Value); // null

```
### Further Reading

* [Ideas](element.schema.ideas.md)