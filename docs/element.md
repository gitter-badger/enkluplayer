# Overview

The `element` system is a barebones node graph with composable state. By itself, it does very little, but by subclassing the `Element` base class, a user may implementing very nice, serializable UI.

### Data

The `ElementDescription` class describes all data for an entire graph of elements. It supports the usual composite structure (node with children that are nodes), in addition to node references, i.e. an entire structure of elements may be referenced multiple times in the same graph.

It does this by providing a reference hierarchy along side a collection of all `ElementData`:

```csharp
public ElementRef Root;
public ElementData[] Elements;
```

The `ElementRef` is a very simple data structure that simply references into the collection of `ElementData`. In this way, the `ElementRef` structure may reference the same `ElementData` multiple times. This structure has the added benefit of disallowing cycles.

Both `ElementRef` and `ElementData` have a list of children and a schema. When constructing the graph, the child lists are combined (putting `ElementData` children first). The `Schema` objects (discussed later) are also combined. Here, the `ElementRef` schema supersedes that of `ElementData`, allowing users to override template data.

### Creation

To create an element structure, use the `ElementFactory`:

```csharp
var element = _factory.Element(description);
```

This will fill out the entire structure of elements. `Element` may not be created directly, but subclasses of `Element` may choose to allow this.

### Schema

The `Schema` object is very interesting. It is a composable grab bag of state. Property objects, rather than property values, are returned from queries. This is important as when polling state on an update, the property object can be cached. The `Value` property can then be polled for the corresponding value.

```csharp
var prop = _schema.Get<int>("foo");

...

void Update() {
  if (prop.Value > 3) {
    ...
  }
}
```

Values may be set using the `Set` method.

```csharp
_schema.Set("foo", 5);
```

`Schema` requests _always_ return a value. Even if you ask for the wrong type, it will return a prop locked to the default value of your bad type.

```csharp
_schema.Set<int>("foo", 5);	// sets foo to 5

var val = _schema.Get<bool>("foo").Value);	// false

_schema.Set("foo", true);	// foo is already an int

val = _schema.Get<bool>("foo").Value);		// false
```

If a element's `Schema` does not have a value, it falls back to the parent, recursively.

```csharp
_root.Set("foo", 5);

...

var val = _greatgrandchild.Get<int>("foo").Value;	// 5
```

However, the decendant may overwrite any of the properties locally.

```csharp
_greatgrandchild.Set("foo", 12);
var val = _greatgrandchild.Get<int>("foo");	// 12

val = _root.Get<int>("foo");				// 5
```

### Events

Adding children dynamically is possible and will fire associated events up the graph.

```csharp
_root.OnChildAdded += (root, child) => {
  ...
};

_greatgrandchild.AddChild(_factory.Element(description));
```