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
// retrieves element
button = app.elements.byId('specific-id');

// destroys element
app.elements.destroy(button);
```

##### Networking (Not Implemented)

```javascript
var element = app.elements.byId('myElement');
app.network.sync(
	element,
	function(el, evt) {
		// evt.type = 'create' | 'update' | 'move' | delete'

		if (evt.type === 'update') {
			// sync only position
		    if (evt.prop.name === 'position') {
		    	if (evt.prop.prev != evt.prop.next) {
		    		return true;
		    	}
		    }
		}
	    

	    return false;
	});

// ...

app.network.unsync(element);
```