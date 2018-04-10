```javascript
// logging params
log.debug('Foo', 5, obj);

// levels
log.info('Info');
log.warn('Warn');
log.error('Error');
```

```javascript
// vector constructors
vec2(x, y);
vec3(x, y, z);
vec4(x, y, z, w);

// vector constants
v.one;
v.zero;
v.up;
v.right;
v.forward;

// vector math
v.mult(10, v.one);
v.add(a, b);
v.dot(a, b);
v.cross(a, b);
v.len();
```

```javascript
// quaternion constructors
quat(x, y, z, w);
quat(x, y, z); // infers euler

// quaternion constants
q.identity;

// quaternion math
q.eul(x, y, z);
```

```javascript
// transform
this.transform.position = v.zero;
this.transform.rotation = q.identity;
this.transform.scale = v.one;
```

```javascript
// element properties
this.props.set('foo', 5);
this.props.get('foo'); // 5

// watching
function watcher(prev, next) {
    log.info(
        'Changed from {0} -> {1}',
        prev,
        next);
));

this.props.watch('foo', watcher);
this.props.set('foo', 12);  // Changed from 5 -> 12
this.props.set('foo', 12);  // < Will not call watcher if value is unchanged. >
this.props.set('foo', 15);  // Changed from 12 -> 15
this.props.unwatch('foo', watcher);
this.props.set('foo', 5);

this.props.watchOnce('foo', function(prev, next) {
    log.info('This will only be called once!');
    
    // automatically unwatched
});
```

```javascript
// returns root element of scene
var scene = app.scenes.get('myScene');

// creates element
var button = app.elements.create('Button');

// add/remove children
scene.addChild(button);
scene.removeChild(button);

// iterate children
var children = scene.getChildren();
for (var i = 0, len = children.length; i < len; i++) {
    // 
}

// destroy
button.destroy();

// or
scene.destroy(button);
```

```javascript
// queries
var z = a.findOne('..z');
var all = a.find('..(@type==Element)');
```

```javascript
var element = app.elements.byId('myElement');
app.network.sync(element, function(el) {
    // always sync
    return true;
});
app.network.unsync(element);
```
