
Synchronize element updates.

```java
var mp = require('mp');
var self = this;

var ctx;

function enter() {
	// find the element
	var a = self.findOne('..a');
	
	// get the context for this elements
	ctx = mp.context(a);

	// request ownership (this is not required)
	ctx.own(function(error) {
		if (error) {
			log.warning('I cannot own this object : ' + error);
		}
	});

	// synchronize all changes to this prop (does not require ownership)
	ctx.sync('position');
}

function onFoo() {
	// stop synchronizing changes to this prop
	ctx.unsync('position');
}

function onBar() {
	// relinquish ownership (will be called automatically on engine exit or disconnect)
	ctx.forfeit();	
}
```

Create new elements.

```java
var assets = require('asset-library');
var scripts = require('script-library');
var mp = require('mp');
var self = this;

function onFoo() {
	// find the asset we want
	var tree = assets.byName('Tree')[0];
	var rotate = scripts.byName('Rotator')[0];

	// create an element builder and specify the parent
	var builder = mp
		.builder(self)
		.asset(tree)
		.scripts([rotate])
		// this is the default value -- the lifecycle of the session is locked to the session
		.expiration(mp.expiration.SESSION)
		// defaults to NONE -- this value means the element will automatically be owned by this element
		.ownership(mp.ownership.SELF)
		.vec('position', vec3(0, 1, 0))
		.bool('visible', false);
		.string('name', 'This is my element!');
	builder.build(function(error, el) {
		if (error) {
			log.warning('Could not create element: ' + error);
		} else {
			ctx.sync('position', 'rotation', 'scale', 'alpha');
		}
	});
}
```