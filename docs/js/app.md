### App Interface

##### Scenes

```javascript
// retrieves ids of all scenes
var all = app.scenes.all;

// returns root element of scene
var root = app.scenes.root('myScene');
```

##### Elements

```javascript
// creates element
var button = app.elements.create('Button');
app.elements.create('Button', 'specific-id');

// retrieves element
button = app.elements.byId('specific-id');

// destroys element
app.elements.destroy(button);
```

##### Networking (Not Implemented)

```javascript
var element = app.elements.byId('myElement');
app.network.sync(element, function(el, prop, prev, next) {
    // sync only position
    if (prop.name === 'position') {
    	return true;
    }

    return false;
});
app.network.unsync(element);
```
