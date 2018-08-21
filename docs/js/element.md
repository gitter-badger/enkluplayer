### Element API

##### Transform

```javascript
// transform
this.transform.position = v.zero;
this.transform.rotation = q.identity;
this.transform.scale = v.one;
```

##### Queries

```javascript
// queries
var z = this.findOne('..z');
var all = this.find('..(@type==Element)');
```

##### Children

```javascript
// add/remove children
element.addChild(button);
if (element.removeChild(button)) {
	log.info('Successfully removed!');
}

// iterate children
var children = element.children;
for (var i = 0, len = children.length; i < len; i++) {
    // 
}
```

##### Props

```javascript
// get element prop
this.props.getNumber('foo'); // 0
this.props.getString('bar'); // ''
this.props.getBool('fizz'); // false

// get element prop
this.props.setNumber('foo', 5);
this.props.setString('bar', 'fizz');
this.props.setBool('fizz', false);

// watching
function watcher(prev, next) {
    log.info(
        'Changed from {0} -> {1}',
        prev,
        next);
));

this.props.watchNumber('foo', watcher);
this.props.setNumber('foo', 12);  // Changed from 5 -> 12
this.props.setNumber('foo', 12);  // < Will not call watcher if value is unchanged. >
this.props.setNumber('foo', 15);  // Changed from 12 -> 15
this.props.unwatchNumber('foo', watcher);
this.props.setNumber('foo', 5);

this.props.watchNumberOnce('foo', function(prev, next) {
    log.info('This will only be called once!');
    
    // automatically unwatched
});
```

##### Destroy

```javascript
// destroys element and all children
element.destroy();
```

##### Send

```javascript
var menu = this.find('..menu-new');

// sends 'open' message to all attached scripts
menu.send('open');
menu.send('register', 'foo', 'bar', 5); // register('foo', 'bar', 5);
```

##### msgMissing

```javascript
function msgMissing(type, args) {
	log.info(type + ' was called but I could not handle it.');
}
```

##### Events

```javascript
a.on('activated', function(evt) {
	// 
});

a.off('activate', onActivated);
```

##### Create

```javascript
// creates element as a child
var a = element.create('Button');
var b = element.create('Button', 'specific-id');

// creates elements from a vine, as a child
var c = element.createFromVine('<Button />');
```