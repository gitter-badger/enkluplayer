### Element

**Strings**

| Property Name | Default        | Description                                                  | Inherit |
| ------------- | -------------- | ------------------------------------------------------------ | ------- |
| font          | Watchword_bold | The name of the font to use for text.                        | No      |
| name          |                | The name of the element. Visible in Enklu Web and Enklu HoloLens. | No      |
| description   |                | A short description of the element. Visible in Enklu Web.    | No      |

**Vectors**

| Property Name | Default   | Description                          | Inherit |
| ------------- | --------- | ------------------------------------ | ------- |
| position      | (0, 0, 0) | Local position of the element.       | No      |
| rotation      | (0, 0, 0) | Local Euler rotation of the element. | No      |
| scale         | (1, 1, 1) | Local scale of the object.           | No      |



### Button

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

