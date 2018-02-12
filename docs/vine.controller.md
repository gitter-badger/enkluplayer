### Overview

When using vines to author IUX in Unity, there are a few primitives and best practices that can help accelerate development.

#### InjectableIUXController

We don't stick to a strict `M[MV]*C?` approach to developing UI. However, we generally do need some sort of "controller" that can create elements from a vine and pull out the objects that we need to interact with. To that end, we have provided `InjectableIUXController`. This object is responsible for loading a vine, creating elements, and pulling out references to requested elements automatically.

#### Walkthrough

Imagine we have the following vine for a menu:

```html
<Menu id='menu' showBackButton=true>
	<Button id='btn-play' label='Play' icon='play' />
	<Button id='btn-new' label='New' icon='plus' />
	<Button id='btn-clearall' label='Clear All' icon='trash' />
	<Button id='btn-quit' label='Quit' icon='x' />
	<Toggle id='toggle-debugrender' label='Debug Render' />
</Menu>
```

We can save this vine as a text asset and add a reference to the asset to the `VineTable` object in the main scene.

![Vine Table](vinetable.png)

Next, we make a new object that extends `InjectableIUXController`. The first thing we need to do is append the `InjectVine` attribute to our class.

```csharp
[InjectVine("MainMenu")]
public class MenuController : InjectableIUXController
{
	// 
}
```

The identifier we pass to `InjectVine` should match the identifier field in `VineTable`.

At this point, if we were to use `AddComponent`, we would see that the elements would automatically be created and added as children to the component's `GameObject`.

```csharp
var root = new GameObject("Main Menu");
var controller = root.AddComponent<MenuController>();
```

Next, we'll want our `MenuController` to pull out some specific objects we'll need to access. We can do this via the `InjectElements` attribute.

```csharp
[InjectVine("Design.MainMenu")]
public class MainMenuController : InjectableIUXController
{
	[InjectElements("..btn-play")]
	public ButtonWidget BtnPlay { get; private set; }

	[InjectElements("..(@type==ButtonWidget)")]
	public ButtonWidget[] AllButtons {get; private set;}

	[InjectElements("..toggle-debugrender")]
	public ToggleWidget ToggleDebugRender { get; private set; }	
}
```

This attribute accepts a [query](element.query.md) and can return either a single element or an array of elements.

All of this happens in the `Awake` method, meaning that the elements are accessible very early. Feel free to override `Awake` or put initialization in `Start`.

```csharp
protected override void Awake()
{
  base.Awake();
  
  BtnPlay.Activator.OnActivated += _ => Debug.Log(this, "Play was pressed!");
}
```

Finally, the `InjectableIUXController` keeps a reference to the root element that was created, accessible via the `Root` property. It then provides some handy integration with Unity lifecycle methods.

`OnEnable` sets visibility of the root element to true, `OnDisable` sets it to false. `OnDestroy` destroys the root element.