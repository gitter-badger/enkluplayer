### Overview

The `Element` system is a barebones node graph system with composable state, called `Schema`. By itself, Element does very little, but through subclassing, a user may implement a variety of responsive systems, quite quickly.

#### Children

Elements are very similar to other object graph systems out there. Each Element has a list of child elements that can be manipulated through `AddChild` and `RemoveChild`.

```csharp
print(a.Children.Length); // 0
a.AddChild(b);
print(a.Children.Length); // 1

c.AddChild(b);
print(a.Children.Length); // 0
```

Events are provided that are propagated up the graph.

```csharp
_root.AddChild(child);
child.AddChild(grandchild);
child.AddChild(greatgrandchild);

_root.OnChildAdded += (parent, child) => {
  print("$child.Id was added to $parent.Id");
};

greatgrandchild.AddChild(newChild);	// newChild was added to greatgrandchild
```

Removing children will do the same.

```csharp
_root.OnChildRemoved += (parent, child) => {
  print("$child.Id was removed from $parent.Id");
}

greatgrandchild.RemoveChild(newChild);	// newChild was removed from greatgrandchild
```

The removed element also receives an event upon its own removal.

```csharp
newChild.OnRemoved += @this => print("I was removed!");
greatgrandchild.RemoveChild(newChild);	// I was removed!
```

#### Queries

Somebody please think of the children!

What happens when an element structure gets... complicated? Finding the elements you need in a sprawling graph is not always easy. Luckily, there's a query language built into the `Element` system.

```csharp
var buttons = _root.Find("..container.(@type==Button)");
```

Yes please, I'll take that. This is but a taste-- for more, check out the [ɛql documentation](element.query.md).

#### Schema

Each `Element` has an `ElementSchema` that allows elements to compose data down the hierarchy.

```csharp
_root.Schema.Set("foo", 12);

greatgrandchild.Schema.Get<int>("foo");
print(foo.Value);	// 12
```

This short example just scratches the surface of our powerful Schema system, so please read more about it in the [Schema documentation](element.schema.md).

#### Creating and Destroying Elements

To create an element structure, pass an `ElementDescription` object to an `IElementFactory`:

```csharp
var element = _factory.Element(description);
```

`StandardElementFactory` (which you should _definitely_ subclass) will fill out the entire structure of elements, bottom up. This is covered in more depth below.

But wait! `IElementFactory` also allows creation of objects _directly from a Vine_.

```csharp
var element = _factory.Element(@"<Cursor />");
```

Oh yes. Yes, my friend, you can create elements through an HTML-ish markup language. There's even a JS preprocessor and C style comments instead of those nasty `<!-- -->` things. This is the preferred method for creating elements. For more details on VineML, please start with the [Vine documentation](vine.overview.md).

#### Lifecycle

Elements are meant to be created via an `IElementFactory` implementation, pooled, and reused with a predictable lifecycle. The base `Element` class has two main API functions, generally called by an `IElementFactory` implementation:

```csharp
Load(ElementData data, 	ElementSchema schema, Element[] children)
Unload()
```

In between these methods, `FrameUpdate` is meant to be called on every frame.

```csharp
element.Load(...)
  while (isRunning) {
    element.FrameUpdate(dt);
    ...
  }
element.Unload()
```

The `Load` method is the initialization method for elements. As may be inferred, the signature of `Load` requires that _allocation is bottom up_, i.e., children are created and `Load`'d first. When `Load` is called on an element, `IElementFactory` should guarantee that `Load` has been completed for each of its children. The `StandardElementFactory` makes this guarantee and is strongly recommended.

The `Load` method itself is not marked as virtual, therefore subclasses cannot override it. However, two methods are provided for override during the load phase: one is called before children are added, and one after.

```csharp
Load()
	-> LoadInternalBeforeChildren()
	-> // Add Children
	-> LoadInternalAfterChildren()
```

Similarly on `Unload`, there are methods both before and after children are unloaded.

```csharp
Unload()
	-> UnloadInternalBeforeChildren()
	-> // Unload Children
	-> UnloadInternalAfterChildren()
```

While the `IElementFactory` takes care of calling `Load` on child elements, `Element` is responsible for calling `Unload` on its children. This is taken care of in `Unload` and why it too is not marked as virtual.

In short, if an `Element` cares about its children, it should initialize in `LoadInternalAfterChildren` and uninitialize in `UnloadInternalBeforechildren`. Otherwise, it should use the other pair of methods.

### Example

In this example, we create an element that collects all of its children and sums their `value` properties.

```csharp
/// <summary>
/// Example element that sums child elements by their "value" property.
/// </summary>
public class SumElement : Element
{
	/// <summary>
	/// Integral sum.
	/// </summary>
	public int Sum { get; private set; }
	
	/// <summary>
	/// Called when the sum changes.
	/// </summary>
	public event Action OnSumChanged;

	/// <inheritdoc />
	protected override void LoadInternalAfterChildren()
	{
		base.LoadInternalAfterChildren();
		
		// children are guaranteed to be added and initialized here
		SumChildren();

		// listen for child updates
		OnChildAdded += This_OnChildUpdated;
		OnChildRemoved += This_OnChildUpdated;
	}

	/// <inheritdoc />
	protected override void UnloadInternalBeforeChildren()
	{
		base.UnloadInternalBeforeChildren();

		// remove listeners before all the children are unloaded
		OnChildAdded -= This_OnChildUpdated;
		OnChildRemoved -= This_OnChildUpdated;
	}

	/// <summary>
	/// Sums children by their "value" property.
	/// </summary>
	private void SumChildren()
	{
		var total = 0;
		foreach (var child in Children)
		{
			total += child.Schema.Get<int>("value").Value;
		}

		if (Sum != total)
		{
			Sum = total;

			if (OnSumChanged != null)
			{
				OnSumChanged();
			}
		}
	}

	/// <summary>
	/// Called when a child has been added or removed.
	/// </summary>
	/// <param name="parent">The parent.</param>
	/// <param name="child">The child.</param>
	private void This_OnChildUpdated(Element parent, Element child)
	{
		if (parent == this)
		{
			SumChildren();
		}
	}
}
```

### ElementDescription

The `ElementDescription` class describes all data for an entire graph of elements. It supports the usual composite structure (node with children that are nodes), in addition to node references, i.e. an entire structure of elements may be referenced multiple times in the same graph.

It does this by providing a reference hierarchy along side a collection of all `ElementData`:

```csharp
public ElementRef Root;
public ElementData[] Elements;
```

The `ElementRef` is a very simple data structure that simply references into the collection of `ElementData`. In this way, the `ElementRef` structure may reference the same `ElementData` multiple times. This structure has the added benefit of disallowing cycles.

Both `ElementRef` and `ElementData` have a list of children and a schema. When constructing the graph, the child lists are combined (putting `ElementData` children first). The `Schema` objects (discussed later) are also combined. Here, the `ElementRef` schema supersedes that of `ElementData`, allowing users to override template data.

### Further Reading

* More about elements:
  * [Queries](element.query.md)
  * [Schema](element.query.md)
  * [Widgets](element.widget.md)
  * [Ideas](element.ideas.md)
* Create elements with VineML:
  * [VineML](vine.overview.md)
  * [Vine Controllers](vine.controller.md)