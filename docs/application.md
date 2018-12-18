### Overview

At a high level, EnkluPlayer is a single object, `Application`, that manages a set of long running services that derive from `ApplicationService`.

##### Application

An instance of `Application` is created in `Main.cs`. We use a dependency injection framework to do this-- the details are not covered here. This object is incredibly simple. It contains an `IApplicationServiceManager` that it forwards function calls to. These functions are the essential lifecycle functions of a Unity application.

Inside the `ApplicationServiceManager`, there is a collection of `ApplicationService` objects. These are objects that run for the lifetime of the application and contain the meat of the application logic.

##### ApplicationService

These objects are designed to be created and once and used once in a simple flow: `Start` -> `Update` -> `Stop`. Internally, the `ApplicationService` also provides a succinct wrapper for the [messaging system](http://docs.enklu.com/commons-unity/index.html#messaging) that automatically unsubscribes from messages on teardown.

In the [services](https://github.com/enklu/enkluplayer/tree/master/Assets/Source/Application/Services) folder, there are many default services. The [`AssetUpdateService`](https://github.com/enklu/enkluplayer/blob/master/Assets/Source/Application/Services/AssetUpdateService.cs) listens for updates to assets and pushes them through the [asset system](assets.overview.md). The [`EnvironmentUpdateService`](https://github.com/enklu/enkluplayer/blob/master/Assets/Source/Application/Services/EnvironmentUpdateService.cs) listens for environment connection changes and writes them to disk for next startup. The [`MetricsUpdateService `](https://github.com/enklu/enkluplayer/blob/master/Assets/Source/Application/Services/MetricsUpdateService.cs) keeps track of various runtime metrics and can send them through the [`metrics`](metrics.overview.md) system. These are all examples of long-running services that need to run for the lifetime of the application.

##### ApplicationStateService

Arguably the most important of these services is the [`ApplicationStateService`](https://github.com/enklu/enkluplayer/blob/master/Assets/Source/Application/Services/ApplicationStateService.cs). This service wraps a simple state machine that contains the major states of the application.  This object deserves its own [documentation](application.stateservice.md) for further reading.