# Interface

### AssetInfo

#### Description

Describes a single asset.

```
class AssetInfo
{
    public string GUID;
    public string URI;
    public int Version;
    public string CRC;
    publis string[] Tags;
}
```

### AssetManager

#### Initialization

```
// configure
var configuration = new AssetManagerConfiguration(map)
{
	Loader = new StandardAssetLoader(),
	Queries = new StandardQueryResolver(),
	Service = new WebSocketAssetUpdateService()
};

// use configuration to initialize
assetManager
    .Initialize(configuration)
    .OnSuccess(_ => ...);
```

### AssetReference

#### Retrieval

```
// retrieve AssetReference by asset guid
var assetRef = assets.Manifest.Get(guid);

// build queries
// Commas are ANDs, spaces are ORs, !s are NOTs.
// 
// Egs-
// 
// "a" will resolve true for a set of tags including "a"
// "a b" will resolve true for a set of tags including "a" OR "b"
// "a,b" will resolve true for a set of tags including "a" AND "b"
// "!a" will resolve true for a set of tags NOT including "a"
// 
// These can be composed for powerful effect:
// 
// "a b,!n,z !!b" is equivalent to: "(a || b) && !n && (z || b)"

// retrieve AssetReference by query
var b = assets.Manifest.FindOne(query);

// retrieve many AssetReferences by query
var refs = assets.Manifest.FindAll(query);
```

#### Get Loaded Asset

```
// get asset
var asset = assetRef.Asset<GameObject>();

// pull off components
var component = assetRef.Asset<MyMonoBehaviour>();
```

#### Load Asset

```
// load an asset
assetRef
    .Load<T>()
    // automatically calls Asset<T> to type return
    .OnSuccess(value => ...);
```

#### Watch for AssetReference Updates From a Closure

```
// safely use a closure
assetRef.Watch(unwatch => {
    ...
    
    unwatch();
});
```

#### Watch for AssetReference Updates Externally

```
// unsubscribe externally to closure
var unwatch = assetRef.Watch(watchedAssetRef => ...);

...

unwatch();

// useful for class level handlers
_unwatch = assetRef.Watch(AssetRef_Watch);

```

#### Watch for Asset Updates

```
// similar to Watch
assetRef.WatchAsset<T>((unwatch, asset) => ...);

// similar to Watch
var unwatch = assetRef.WatchAsset<T>(asset => ...);
```

#### Reloading

```
// manually
assetRef.Watch(unsub => assetRef.Load<Object>());

// automatically
assetRef.AutoReload = true;

// either of the above actions may call WatchAsset<T>
```

#### EXPERIMENTAL: Initializers

```
// register intitializers
foreach (var reference in _assets.Find(Tags.Runnable))
{
    reference.Use<IRunnable>((asset, next) => {
        asset
            .Run()
            .OnFinally(_ => next());
    });  
}

...

// elsewhere in the codebase, for MyThing : IRunnable
var reference = _assets.Get(guid);
reference
    .Load<MyThing>()
    .OnSuccess(thing =>
    {
        // don't need to worry about initializing
        
        thing.Foo();
    });
```
