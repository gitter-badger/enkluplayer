# Overview

**Enklu Player** is an Augmented Reality (AR) runtime for UWP, WebGL, Android, and iOS.

* **Highly Iterative**: Enklu Player gives users instant feedback. Change layout, assets, UI, or scripts on the fly. There's no need to rebuild-- Enklu Player will automatically download the newest data.
* **Truly Cross Platform**: Different platforms have different needs, and Enklu Player doesn't just compile to multiple targets, it allows users to tailor AR experiences to multiple platforms.
* **Out of the Box Networking**: Enklu Player provides networking support with our cloud services or your own. Multiple users can edit an application together in realtime on separate devices, while many users simultaneously play through an experience. Collaboration and social engagement are pillars of Enklu Player.

### Getting Up and Running

To develop with Enklu Player, you will need a few prerequisites.

* Requires **Unity 2018.1.1**.
* Setup your [ApplicationConfig.json](docs/applicationconfig.md) file.
* Enklu Player does not require an Enklu account, but it is best used with an [Enklu Web](https://editor.enklu.com) account. An account is required to use Enklu Player's builtin multi-user editing and realtime multiplayer features.

### Building

##### iOS

Until we add support via the `Unity.iOS.Xcode.PBXProject` interface, you have to do a few things by hand:
    * General > Signing - select valid team.
    * _info.plist_ - Add:

```
<key>LSApplicationQueriesSchemes</key>
<array>
    <string>instagram</string>
</array>
```

And

```
Privacy - Photo Library Usage Description
```
```
NSPhotoLibraryAddUsageDescription
```

    * Build Phases > Link Binary with Libraries - Add Photos.framework.

##### HoloLens

*To enable crash logging*, we add a small bit of code to the generated `App.cs` file:

```csharp
CoreApplication.UnhandledErrorDetected += (sender, eventArgs) =>
{
	try
	{
		eventArgs.UnhandledError.Propagate();
	}
	catch (Exception exception)
	{
		Log.Fatal(exception, exception);
	}
};
```

### System Documentation

* [AssetManager](docs/assets.overview.md)
  * For details on how assets are uploaded, imported, and served, see [this diagram](https://www.lucidchart.com/documents/view/dd316cb9-5b27-4e67-8829-e508d91b4e79).
  * [Ideas](docs/assets.ideas.md)
* [Scripting](docs/scripting.overview.md)
  * [Ideas](docs/scripting.ideas.md)
* [Element](docs/element.overview.md)
  * [Queries](docs/element.query.md)
  * [Schema](docs/element.schema.md)
    * [Element Properties](docs/element.schema.properties.md)
  * [Widget](docs/element.widget.md)
  * [Ideas](docs/element.ideas.md)
* [Vine](docs/vine.overview.md)
  * [Controllers](docs/vine.controller.md)
* Miscellany
  * [Metrics](docs/metrics.overview.md)
  * [Standard Queries](docs/standardqueries.overview.md)
* [Trellis API](docs/trellis.api.md)

### Contributing

Enklu is dedicated to providing the most highly polished AR workflow to the world, and we firmly believe the best way to do that is out in the open. If you want to take part, please read through these sections to learn how you can help make Enklu Player even better.

All contributors must read and agree to our [license](LICENSE.md) and [code of conduct](docs/codeofconduct.md).

Read our [contribution guide](CONTRIBUTING.md) to learn what types of contributions we're after as well as how to best direct your energy. This guide covers creating issues, resolving bugs, and proposing new features.