### Overview

Each different element type reacts to props in different ways. This document outlines the different props used.

#### Element

**Strings**

| Property Name | Default        | Description                                                  | Inherit |
| ------------- | -------------- | ------------------------------------------------------------ | ------- |
| font          | Watchword_bold | The name of the font to use for text.                        | No      |
| name          |                | The name of the element. Visible in Enklu Web and Enklu HoloLens. | No      |
| description   |                | A short description of the element. Visible in Enklu Web.    | No      |
| focus         |                | If set to "Camera", the element will billboard.              | No      |

**Vectors**

| Property Name | Default   | Description                          | Inherit |
| ------------- | --------- | ------------------------------------ | ------- |
| position      | (0, 0, 0) | Local position of the element.       | No      |
| rotation      | (0, 0, 0) | Local Euler rotation of the element. | No      |
| scale         | (1, 1, 1) | Local scale of the object.           | No      |



#### Button

**Numbers**

| Property Name            | Default | Description                                                  | Inherit |
| ------------------------ | ------- | ------------------------------------------------------------ | ------- |
| font.size                | 70      | Determines the font size of the button's label.              | No      |
| icon.scale               | 1       | Icon scale multiplier.                                       | No      |
| label.padding            | 60      | Determines distance between center of circle and label.      | No      |
| aim.multiplier           | 1       | Scalar for aim, which is used to determine how a button is activated. | No      |
| stability.multiplier     | 1       | Scalar for stability, which is used to determine how a button is activated. | No      |
| fill.duration.multiplier | 1       | Scalar for fill duration, used to extend or shorten how long it takes to activate a button. | No      |
| ready.frameScale         | 1       | Scale of the button's frame while in the ready state.        | No      |
| activating.frameScale    | 1       | Scale of the button's frame while in the activating state.   | No      |
| activated.frameScale     | 1       | Scale of the button's frame while in the activated state.    | No      |



**Strings**

| Property Name           | Default       | Description                                                  | Inherit |
| ----------------------- | ------------- | ------------------------------------------------------------ | ------- |
| src                     |               | URL for icon.                                                | Yes     |
| icon                    |               | Name of icon, as given in icon list.                         | Yes     |
| label                   |               | The button's label.                                          | Yes     |
| layout                  | *horizontal*  | Describes how the button is laid out. Can be set to *horizontal* or *vertical*. If set to anything other than *vertical*, the layout will be *horizontal*. | Yes     |
| voiceActivator          |               | If set to a keyword, adds voice recognition for activating a button. | Yes     |
| ready.color             | *Ready*       | The VirtualColor of the button's ready state.                | No      |
| ready.captionColor      | *Primary*     | The VirtualColor of the button label in ready state.         | No      |
| ready.tween             | *Responsive*  | The TweenType of the button in ready state.                  | No      |
| activating.color        | *Interacting* | The VirtualColor of the button's activating state.           | No      |
| activating.captionColor | *Interacting* | The VirtualColor of the button label in activating state.    | No      |
| activating.tween        | *Responsive*  | The TweenType of the button in activating state.             | No      |
| activated.color         | *Interacting* | The VirtualColor of the button's activated state.            | No      |
| activated.captionColor  | *Interacting* | The VirtualColor of the button label in activated state.     | No      |
| activated.tween         | *Responsive*  | The TweenType of the button in activated state.              | No      |

**Vectors**

| Property Name    | Default         | Description                              | Inherit |
| ---------------- | --------------- | ---------------------------------------- | ------- |
| ready.scale      | (1, 1, 1)       | Scale of the button in the ready state.  | No      |
| activating.scale | (1.1, 1.1, 1.1) | Scale of the button in activating state. | No      |
| activated.scale  | (1, 1, 1)       | Scale of the button in activated state.  | No      |

#### Toggle

Inherits all `Button` properties.

| Property Name | Type | Default | Description                     | Inherit |
| ------------- | ---- | ------- | ------------------------------- | ------- |
| value         | bool | false   | True iff the toggle is checked. | No      |



#### Image

| Property Name | Type   | Default | Description                                    | Inherit |
| ------------- | ------ | ------- | ---------------------------------------------- | ------- |
| src           | string |         | image URL to download from.                    | Yes     |
| sprite        | Sprite |         | Useful in code-- can directly assign a Sprite. | Yes     |
| width         | float  | 0       | Image width.                                   | No      |
| height        | float  | 0       | Image height.                                  | No      |

#### Text (Caption)

| Property Name | Type   | Default | Description                                                  | Inherit |
| ------------- | ------ | ------- | ------------------------------------------------------------ | ------- |
| label         | string |         | Text string to render.                                       | Yes     |
| fontSize      | int    | 0       | Size of the font.                                            | Yes     |
| width         | float  | 0       | Width before overflow takes over.                            | Yes     |
| alignment     | string |         | Determines how text is aligned within width (MidRight, MidCenter, MidLeft, TopRight, TopCenter, TopLeft, BotRight, BotCenter, BotLeft). | Yes     |
| overflow      | string |         | Determines how text is laid out after width is met (Overflow or Wrap). | Yes     |

#### Menu

| Property Name   | Type   | Default | Description                                                  | Inherit |
| --------------- | ------ | ------- | ------------------------------------------------------------ | ------- |
| title           | string |         | Title of the menu.                                           | Yes     |
| description     | string |         | Description field used in the menu.                          | Yes     |
| fontSize        | int    | 80      | Size of the font.                                            | Yes     |
| layout          | string | Radial  | Determines the type of layout for all child elements. Only Radial supported for now. | Yes     |
| layout.degrees  | float  | 25      | Degrees offset between menu options.                         | Yes     |
| layout.radius   | float  | 0.8     | Distance of menu options from the central hub.               | Yes     |
| header.width    | int    | 700     | The width of the title and description header.               | Yes     |
| header.padding  | int    | 0       | The distance between the header and the menu options.        | Yes     |
| showBackButton  | bool   | false   | If true, shows a back button to the left of the menu header. | Yes     |
| divider.offset  | float  | 0       | Offsets the divider between header and menu options.         | Yes     |
| divider.visible | bool   | true    | If true, shows a divider between header and menu options.    | Yes     |
| page.size       | int    | 4       | How many options are visible on a single page.               |         |

#### SubMenu

Inherits all `Menu` properties.

| Property Name | Type   | Default | Description                                                  | Inherit |
| ------------- | ------ | ------- | ------------------------------------------------------------ | ------- |
| label         | string |         | The label displayed on the button while the menu is collapsed. | No      |
| icon          | string |         | The name of the icon as specified in the [icons](element.icons.md) documentation. | No      |

#### Float

| Property Name  | Type  | Default   | Description                                                  | Inherit |
| -------------- | ----- | --------- | ------------------------------------------------------------ | ------- |
| focus          | vec3  | (0, 0, 0) | The location of the focus sphere relative to the Float.      | Yes     |
| focus.visible  | bool  | true      | Determines whether or not the focus sphere is visible.       | Yes     |
| focus.reorient | float | 3.5       | This value determines how far from the center of the Float a user's gaze may get before the Float reorients itself. | Yes     |

#### Screen

| Property Name | Type  | Default | Description                                                  | Inherit |
| ------------- | ----- | ------- | ------------------------------------------------------------ | ------- |
| distance      | float | 1       | Determines how far away from the camera the element will be locked. | No      |

#### Slider

| Property Name | Type   | Default | Description                                                  | Inherit |
| ------------- | ------ | ------- | ------------------------------------------------------------ | ------- |
| length        | float  | 0.1f    | How long the slider should be.                               | Yes     |
| axis          | string | x       | Determines the axis of the slider: x, y, or z.               | Yes     |
| tooltip       | bool   | false   | Determines whether or not a tooltip should be displayed for this slider. | Yes     |

#### Select

No special properties.

#### Option

| Property Name | Type   | Default | Description                            | Inherit |
| ------------- | ------ | ------- | -------------------------------------- | ------- |
| label         | string |         | The label used in parent elements.     | Yes     |
| value         | string |         | The value associated with this option. | Yes     |

#### OptionGroup

| Property Name | Type   | Default | Description                            | Inherit |
| ------------- | ------ | ------- | -------------------------------------- | ------- |
| label         | string |         | The label used in parent elements.     | Yes     |
| value         | string |         | The value associated with this option. | Yes     |

#### Grid

| Property Name      | Type  | Default | Description                                                 | Inherit |
| ------------------ | ----- | ------- | ----------------------------------------------------------- | ------- |
| padding.vertical   | float | 0.15    | Determines vertical space between elements in the grid.     | Yes     |
| padding.horizontal | float | 0.15    | Determines horizontal spacing between elements in the grid. | Yes     |

#### Asset (Content)

| Property Name | Type   | Default | Description                             | Inherit |
| ------------- | ------ | ------- | --------------------------------------- | ------- |
| assetSrc      | string |         | Id of AssetData.                        | No      |
| scripts       | json   | []      | Comma delimited list of ScriptData ids. | No      |

#### Transition

| Property Name | Type   | Default    | Description                                                  | Inherit |
| ------------- | ------ | ---------- | ------------------------------------------------------------ | ------- |
| prop          | string | alpha      | The name of the prop to tween from start to end.             | No      |
| start         | float  | 0          | The value at which to start the tween.                       | No      |
| end           | float  | 1          | The value at which to end the tween.                         | No      |
| tween         | string | Pronounced | The TweenType: Instance, Responsive, Deliberate, or Pronounced. | No      |

#### Light

| Property Name | Type   | Default      | Description                                                  | Inherit |
| ------------- | ------ | ------------ | ------------------------------------------------------------ | ------- |
| lightType     | string | Directional  | The type of light: Point, Spot, or Directional.              | No      |
| intensity     | float  | 1            | The intensity of the light.                                  | No      |
| shadows       | string | None         | The type of shadows this light should cast: None, Soft, or Hard. | No      |
| color         | col4   | (1, 1, 1, 1) | The color of the light.                                      | No      |
| point.range   | float  | 1            | The range of a point light.                                  | No      |
| spot.range    | float  | 1            | The range of a spot light.                                   | No      |
| spot.angle    | float  | 30           | The angle of spot light.                                     | No      |

