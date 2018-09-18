### Overview

Enklu Player builds to multiple platforms. This document provides notes on specific needs of specific platforms.

#### iOS

Until we add support via the `Unity.iOS.Xcode.PBXProject` interface, you have to do a few things by hand:
​    * General > Signing - select valid team.
​    * _info.plist_ - Add:

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

```
* Build Phases > Link Binary with Libraries - Add Photos.framework.
```

#### HoloLens

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

### 