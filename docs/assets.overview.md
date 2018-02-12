### Overview

The asset system is made up of static data, data management, and asset loading. This document will focus on usage.

### AssetData

`AssetData` is the entry point to the asset system. If an asset is required, there must first be a corresponding `AssetData`. This object contains a unique id, as well as information about downloading the asset.

### AssetManager

`AssetManager` is the object that manages and loads assets. It keeps a manifest of `AssetData` and is responsible for reloading assets automatically.

##### Initialization

First, the `AssetManager` must be initialized. Take a look at the [StandardQueryResolver](standardqueries.overview.md) is you are unfamiliar.

```csharp
// configure
var configuration = new AssetManagerConfiguration(map)
{
	// this object knows how to load assets
	Loader = new StandardAssetLoader(),
	// this object knows how to find assets
	Queries = new StandardQueryResolver()
};

// use configuration to initialize
assetManager
    .Initialize(configuration)
    .OnSuccess(_ => ...);
```

### AssetManifest

The `AssetManifest` is owned by the `AssetManager` and contains all the `AssetData` and corresponding `Asset` instances. It has methods to add, remove, update, and query `AssetData` and `Asset` objects. The actual method of querying is dictated by the `IQueryResolver` implementation passed into the `AssetManager`. The `StandardQueryResolver` is quite good-- [docs here](standardqueries.overview.md).

##### Updating `AssetData`

The `AssetManifest` itself does not add/remove/or update `AssetData`; it only provides methods to do so. In `SpirePlayer`, the `AssetUpdateService` is a long-running service that watches for changes to `AssetData` and pushes those updates to the manifest.

##### Retrieval

```csharp
// retrieve AssetReference by asset guid
var asset = assets.Manifest.Get(guid);

// retrieve AssetReference by query
var b = assets.Manifest.FindOne(query);

// retrieve many AssetReferences by query
var refs = assets.Manifest.FindAll(query);
```

### Asset

`Asset` is the object that keeps a reference to the underlying Unity asset. It is also the interface for watching asset changes and reloading them.

##### Load Asset

```csharp
// load an asset
assetRef
    .Load<T>()
    // automatically calls Asset<T> to type return
    .OnSuccess(value => ...);
```

##### Get Loaded Asset

```csharp
// get GameObject
var asset = asset.As<GameObject>();

// pull components off instead
var component = asset.As<MyMonoBehaviour>();
```

##### Watch for Asset Updates From a Closure

Watching for `Asset` updates means you can watch for updates to the Unity asset.

```csharp
// safely use a closure
asset.Watch(unwatch => {
    ...
    
    unwatch();
});
```

##### Watch for Asset Updates Externally

```csharp
// unsubscribe externally to closure
var unwatch = assetRef.Watch(watchedAssetRef => ...);

...

unwatch();

// useful for class level handlers
_unwatch = assetRef.Watch(AssetRef_Watch);

```

##### Watch for AssetData Updates

Instead of watching for `Asset` updates, you can also watch for `AssetData` updates.

```csharp
// similar to Watch
assetRef.WatchData<T>((unwatch, asset) => ...);

// similar to Watch
var unwatch = assetRef.WatchData<T>(asset => ...);
```

##### Reloading

Finally, you can mark an `Asset` to automatically reload the underlying Unity asset.

```csharp
// manually
asset.Watch(unsub => asset.Load<Object>());

// or automatically!
asset.AutoReload = true;

// either of the above actions may call Watch<T> handlers
```