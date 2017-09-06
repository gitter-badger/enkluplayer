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
// use configuration to initialize
assetManager
    .Initialize(configuration)
    .OnSuccess(_ => ...);

// configure
var configuration = new AssetManagerConfiguration(map)
{
    UriResolver = new UriResolver(...)
};

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

#### Watch for Updates

```
// safely use a closure
assetRef.Watch(unsub => {
    ...
    
    unsub();
});

// unsubscribe externally to closure
var unsub = assetRef.Watch(watchedAssetRef => ...);

...

unsub();

// useful for class level handlers
_unsub = assetRef.Watch(AssetRef_Watch);

```

#### Initializers
```
// register intitializers
foreach (var reference in _assets.Find(query))
{
    reference.Use((asset, next) => {
        // initialize asset in some way
        ...

        // pass it along
        next();
    });  
}
```
