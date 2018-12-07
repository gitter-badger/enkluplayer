### Platforms

Enkluplayer builds to multiple platforms. This document provides notes on specific needs of specific platforms.

##### iOS

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

### UnityBuilderHooks

We have provided Unity context menu options in `UnityBuilderHooks.cs` for switching platforms and building executables for target platforms.  These methods are also used when building from the command line.

### Gradle

We use [gradle](http://gradle.org) to automate our build process. This, together with a custom gradle plugin (which will be released shortly) allows us to run tests, build from Unity, and in the case of certain platforms (like HoloLens), build the output from Unity.

There is a build configuration for each target platform, which can be run with `gradle -b [file] task`.