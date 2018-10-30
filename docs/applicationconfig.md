### Overview

Configuration settings are applied through the *ApplicationConfig.json* file found in *Resources/ApplicationConfig.json*. A default config is provided in the repository.

#### Platform Specific Configs

Each platform has its own `ApplicationConfig` that overrides any settings in `ApplicationConfig.json`. These configs are given by `ApplicationConfig.[RuntimePlatform].json`. So, for example, both `WebGLPlayer` and `WSAPlayerX86` have separate configurations.

#### Override

You may also specify an override, called *ApplicationConfig.Override.json*, which has been added to the `.gitignore`. This file is layered on top of the platform specific config and allows you to edit config locally without worrying about it accidentally being added to version control.

#### Options

Top level configuration values.

| Name        | Default Value | Description                                                  |
| ----------- | ------------- | ------------------------------------------------------------ |
| Version     | "0.0.0"       | Version of the player. The player automatically checks its version against Enklu's backend services to make sure it is up to date. |
| Platform    | ""            | Any Unity RuntimePlatform may be entered here instead of changing platforms through Unity. The application will then "act like" the target platform. Especially handy for moving between platforms quickly. |
| IuxDesigner | false         | If set to true, all platforms will be forced to use the IUX based scene designer. |

Remaining options are grouped into sub-objects.

##### Log

| Name  | Default Value | Description                                          |
| ----- | ------------- | ---------------------------------------------------- |
| Level | Debug         | Can be set to Debug, Info, Warning, Error, or Fatal. |

##### Play

| Name                   | Default Value | Description                                                  |
| ---------------------- | ------------- | ------------------------------------------------------------ |
| AppId                  | ""            | If set, the application will automatically load into this experience. Only applicable on certain platforms. |
| Edit                   | true          | Determines which mode the HoloLens will default to loading experiences in from the experience load menu. |
| PeriodicUpdates        | false         | If true, the player will only check for experience updates every X minutes, as given by `PeriodicUpdatesMinutes`. If false, the player checks for updates every time the experience is loaded. |
| PeriodicUpdateMinutes  | 0             | The number of minutes to wait between updates.               |
| SkipDeviceRegistration | false         | If true, skips the device registration portion of startup flow. |
| SkipVersionCheck       | false         | If true, skips the version check portion of the startup flow. |

##### Network

| Name                       | Default Value | Description                                                  |
| -------------------------- | ------------- | ------------------------------------------------------------ |
| AssetDownloadLagSec        | 0             | Number of seconds to pad downloads with. This is useful for debugging slow network environments. |
| AssetDownloadFailureChance | 0             | This value, between 0 and 1, determines the chance that an asset will be forced to fail loading. This is useful for debugging spotty networking conditions. |
| AnchorDownloadFailChance   | 0             | This value, between 0 and 1, determines the chance that an anchor will be forced to fail downloading. |
| AnchorImportFailChance     | 0             | This value, between 0 and 1, determines the chance that an anchor will be forced to fail importing. |
| Offline                    | false         | If set to true, the player will disable networking entirely. Only previously cached experiences may be loaded. |
| ApiVersion                 | 0.0.0         | The Trellis API version that the player is compatible with.  |
| Current                    | ""            | The name of the current environment to connect to.           |
| AllEnvironments            | []            | An array of EnvironmentData that the player can connect to.  |
| AllCredentials             | []            | An array of CredentialsData that the player can use to connect to an environment automatically. |

**EnvironmentData**

| Name       | Default Value | Description                                                  |
| ---------- | ------------- | ------------------------------------------------------------ |
| Name       | local         | The name of this environment. This is used to identify the environment so it should be unique. |
| TrellisUrl | localhost     | Url at which to find a Trellis instance.                     |
| AssetsUrl  | localhost     | Url at which to upload assets.                               |
| BundlesUrl | localhost     | Url at which to download bundles.                            |
| ThumbsUrl  | localhost     | Url at which to download thumbnails of assets.               |

**CredentialsData**

| Name        | Default Value | Description                                                  |
| ----------- | ------------- | ------------------------------------------------------------ |
| Environment | ""            | The name of the environment this credential is for.          |
| Email       | ""            | The user's email address. This is used for login.            |
| Password    | ""            | The user's plaintext password. This is used for login, but should not be distributed. |
| Token       | ""            | The user's token. This can be provided instead of an email and password. |
| UserId      | ""            | Unique user id.                                              |

##### Conductor

| Name                 | Default Value | Description                                                  |
| -------------------- | ------------- | ------------------------------------------------------------ |
| BatteryUpdateDeltaMs | 600000        | How often to send the conductor our battery life update. This only applies if the device belongs to an organization with Enklu Conductor access. |

##### Metrics

| Name           | Default Value | Description                                                  |
| -------------- | ------------- | ------------------------------------------------------------ |
| Enabled        | true          | If true, enables the MetricsService.                         |
| Targets        | ""            | Specifies a comma-delimited list of metrics targets. Currently, only `HostedGraphite` and `json` are supported. |
| Hostname       | ""            | The hostname of a metrics gathering service. The default metrics provider is Graphite. |
| ApplicationKey | ""            | The application key associated with the metrics gathering service. The metrics provider will use this to authorize transactions. |

