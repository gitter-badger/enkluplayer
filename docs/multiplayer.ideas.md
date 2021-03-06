
Synchronize element updates.

```javascript
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
		
		// sets the asset
		.asset(tree)
		
		// sets the scripts on this element
		.scripts([rotate])
		
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
		.setCol('menu-color', col4(1, 0, 0, 1))
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

Auto-toggle.

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
