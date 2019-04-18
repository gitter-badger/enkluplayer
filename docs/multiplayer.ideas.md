### Synchronization

```javascript
function activate() {
	// synchronize all changes to the position prop
	ctx.sync('position');
}

function deactivate() {
	// stops synchronizing
	ctx.unsync('position');
}
````


### Building Elements

```javascript
var assets = require('asset-library');
var scripts = require('script-library');
var mp = require('mp');
var self = this;

function onFoo() {
	// find the asset we want
	var tree = assets.byName('Tree')[0];

	// find the script we want
	var rotate = scripts.byName('Rotator')[0];

	// create an element builder
	var builder = mp
		// the builder requires a parent
		.builder(self)
        
        // element type, defaults to asset
        .type(mp.elementTypes.ASSET);
		
		// sets the asset from another element
		.asset(tree)
		
		// this is the default value -- the lifecycle of the element is locked to the session
		.expiration(mp.expiration.SESSION)
		
		// defaults to NONE -- the SELF value means the element will automatically be owned by this element
		.ownership(mp.ownership.SELF)
		
		// some props could have custom functions
		.name('This is my name.')

		// set prop values
		.setVec('position', vec3(0, 1, 0))
		.setBool('visible', false);
		.setString('menu-name', 'I am in my element.')
		.setCol4('menu-color', col4(1, 0, 0, 1))
		.setFloat('foo', 1.2);
	
	// use the builder to atomically build the element
	builder.build(function(error, el) {
		if (error) {
			log.warning('Could not create element: ' + error);
		} else {
			log.info('I created an element!');
		}
	});
}
```

### Auto Toggle

```javascript
var mp = require('mp');
var self = this;

function onTouched() {
	var piece = self.findOne('..bar');
	var ctx = mp.context(piece);
	ctx.autoToggle('isAnimating', true, 30000);
}

function enter() {
	var piece = self.findOne('..bar');
	piece.schema.watchBool('isAnimating', function(prev, next) {
		// play animation
	});
}
```

### Ownership

Not implemented.

```javascript
var mp = require('mp');
var self = this;

var ctx;

function enter() {
	// find the element
	var a = self.findOne('..a');
	
	// get the context for this element
	ctx = mp.context(a);

	// request ownership (disallows other entities from changing element or children)
	ctx.own(function(error) {
		if (error) {
			log.warning('I cannot own this object : ' + error);
		}
	});
}

function onBar() {
	// relinquish ownership (will be called automatically on engine exit or disconnect)
	ctx.forfeit();	
}
```