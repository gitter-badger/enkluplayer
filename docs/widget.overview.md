# Overview

The [Element](element.md) and [Schema](element.schema.md) systems provide simple methods for composing objects and state. Both of these systems have few (if any) dependencies outside of the standard C# library. In order to pull together the Unity hierarchy and the Element system, we have introduced the `Widget` class.

In short, `Widget` is an `Element` with a `GameObject`. Its main responsibilities are to manage that `GameObject` and to compose a few properties nicely-- mainly color and tween-- which are multiplied with their parent rather than using the usual method of `Schema` fallback.

### Element Hierarchy vs Unity Hierarchy

Since `Widget` is an `Element`, you can use `AddChild` and `RemoveChild`. Internally, `Widget` will attempt to attach the child to the Unity scene graph if it's a widget. This will result in the scene graph looking similar to the element hierarchy-- though there are many exceptions.

Let's look at an example. In this vine, we are using a number of widgets: `Float`, `Menu`, `Button`, and `Select`. `Option` is not a widget (it's just data-- an `Element` doesn't need a visual representation).

```html
<?Vine>

<Float>
	<Menu title='Scan'>
		<Button label='New Scan' color=#FFFFFF />
		<Select>
			<Option label='Rendering On' />
			<Option label='Rendering Off' />
		</Select>
		<Button label='Exit' />
	</Menu>
</Float>
```

Let's see what this actually creates in the Unity hierarchy:

![Hierarchy](widget.overview.hierarchy.png)

Er-- close. `Float` is a bit special and attaches children to a special place. `Menu`, on the other hand acts exactly like you'd expect. As I pointed out above, `Option` doesn't even have a `GameObject`.

#### Writing a Widget: Best Practices

`Widget` is made for subclassing. At this point, there are many examples of `Widget` subclasses (of varying quality) that you can read through. `Caption` and `ButtonWidget` are probably the most straightforward. Through these, you will see a number of best practices:

* Grab references to all needed schema props in `LoadInternalAfterChildren`. You will generally want to listen to changes for every property you retrieve.
* Remove listeners in one of the unload methods.
* If your `Widget` needs to create other elements internally, use `AddChild` rather than parenting them directly to the owned `GameObject`. This will ensure that the element is properly unloaded and destroyed automatically.
* If you need children to be added to a specific `Transform` rather than the `GameObject` `Transform`, override `GetChildHierarchyParent`. The `FloatWidget` is a good example of this.