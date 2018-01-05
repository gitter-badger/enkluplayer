# Overview

`SpirePlayer` is a basic player for UWP, WebGL, Android, iOS, embedded devices, and anything else we can get our hands on.

### Getting Up and Running

To develop with `spireplayer`, you will need a few prerequisites.

* Requires **Unity 2017.2.x**.
* Download and install the latest version of [node.js](https://nodejs.org/en/download/).
* Setup the [spire-react](https://github.com/create-ar/spire-react) project as shown in the `spire-react` [getting started guide](https://github.com/create-ar/spire-react/blob/master/docs/gettingstarted.md).
* Setup the [spire-trellis](https://github.com/create-ar/spire-trellis) project as shown in the `spire-trellis` [getting started guide](https://github.com/create-ar/spire-trellis/blob/master/docs/gettingstarted.md).

Run both the spire-react and spire-trellis projects.

Run `spireplayer` in Unity. In the Spire Editor, be sure to setup the context to connect to `localhost` as shown in the [context documentation](https://github.com/create-ar/spire-react/blob/master/docs/contexts.md).

### Event Flow

In order to understand how `SpirePlayer` works, it's essential to understand how events flow through the application. In particular, much time has been spent on abstracting away the Spire Editor as the controlling portal for the device.

A detailed diagram can be found [here](https://www.lucidchart.com/documents/view/971681eb-a74f-4ab7-a33c-5f3509065f2b).

### Systems of Interest

* [AssetManager](docs/assets.md)
  * For details on how assets are uploaded, imported, and served, see [this diagram](https://www.lucidchart.com/documents/view/dd316cb9-5b27-4e67-8829-e508d91b4e79).
* [Scripting](docs/scripting.overview.md)
* ~[ContentGraph](docs/contentgraph.md)~ Obsolete
* [Element](docs/element.md)
  * [Queries](docs/element.query.md)

