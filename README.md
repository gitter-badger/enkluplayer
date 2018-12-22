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
* See our documentation on [building](docs/building.md) for creating device builds.

### System Documentation

* [AssetManager](docs/assets.overview.md)
  * [Ideas](docs/assets.ideas.md)
* [Scripting](docs/scripting.overview.md)
  * [Ideas](docs/scripting.ideas.md)
* [Element](docs/element.overview.md)
  * [Queries](docs/element.query.md)
  * [Schema](docs/element.schema.md)
    * [Element Properties](docs/element.schema.properties.md)
  * [Widget](docs/element.widget.md)
  * [Tweening](docs/tween.overview.md)
  * [Ideas](docs/element.ideas.md)
* [Vine](docs/vine.overview.md)
  * [Controllers](docs/vine.controller.md)
* Miscellany
  * [Metrics](docs/metrics.overview.md)
  * [Standard Queries](docs/standardqueries.overview.md)
* [Trellis API](docs/trellis.api.md)
* [JavaScript API](http://docs.enklu.com/jsapi-v0.4.0/index.html)
* Common Libraries
  * [API Documentation](http://docs.enklu.com/commons-unity/index.html)
  * [Logging](https://github.com/enklu/commons-unity-logging)
  * [Async](https://github.com/enklu/commons-unity-async)
  * [Http](https://github.com/enklu/commons-unity-http)
  * [Debug Rendering](https://github.com/enklu/commons-unity-debugrendering)
  * [Messaging](https://github.com/enklu/commons-unity-messaging)

### Contributing

Enklu is dedicated to providing the most highly polished AR workflow to the world, and we firmly believe the best way to do that is out in the open. If you want to take part, please read through these sections to learn how you can help make Enklu Player even better.

All contributors must read and agree to our [license](LICENSE.md) and [code of conduct](docs/codeofconduct.md).

Read our [contribution guide](CONTRIBUTING.md) to learn what types of contributions we're after as well as how to best direct your energy. This guide covers creating issues, resolving bugs, and proposing new features.

To get started, checkout out issues with the label [good first issue](https://github.com/enklu/enkluplayer/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22). These are great for digging into the codebase.
