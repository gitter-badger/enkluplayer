# Overview

`SpirePlayer` is a basic player for UWP, WebGL, Android, iOS, and anything else we can get our hands on.

### Event Flow

In order to understand how `SpirePlayer` works, it's essential to understand how events flow through the application. In particular, much time has been spent on abstracting away the Spire Editor as the controlling portal for the device.

A detailed diagram can be found [here](https://www.lucidchart.com/documents/view/971681eb-a74f-4ab7-a33c-5f3509065f2b).

### Systems of Interest

* [AssetManager](docs/assets.md)
	* For details on how assets are uploaded, imported, and served, see [this diagram](https://www.lucidchart.com/documents/view/dd316cb9-5b27-4e67-8829-e508d91b4e79).
* [Scripting](docs/scripting.overview.md)
* [ContentGraph](docs/contentgraph.md)
