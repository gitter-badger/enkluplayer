# Interface

### AssetMap

#### Description

Collection of AssetInfo.

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

#### Setup

```
var map = new AssetMap();
map.Add(info);
map.Add(infos);
```

#### Usage

```
var info = map.Info(guid);
```

### AssetManager

#### Initialization

```
// configure
var configuration = new AssetManagerConfiguration(map)
{
    UriResolver = new UriResolver(...)
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
// commas are ANDs, spaces are ORs, exclamation points are NOTs
var query = "tag anotherTag,yetAnotherTag,!finalTag";
// equivalent to
//      (tag || anotherTag) && (yetAnotherTag) && (!finalTag)

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

#### Watch for AssetReference Updates

```
// safely use a closure
assetRef.Watch(unwatch => {
    ...
    
    unwatch();
});

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
assetRef.Watch(unsub => assetRef.Load());

// automatically
assetRef.AutoReload = true;

// either of the above actions may call WatchAsset<T>
```

#### Initializers

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
