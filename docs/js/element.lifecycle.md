### Element Lifecycle Methods

##### Initialization

Code can be written outside of any function, which will be called immediately.

```javascript
var foo = 5;

// scripts will be executed _after_ all VineML scripts
var button = this.find('..btn-foo');
```

##### Enter (Partially Implemented)

Called as part of FSM flow after all scripts have been initialized. This is also guaranteed to be called after IUX scripts on the same element.

```javascript
function enter() {
	log.info('Enter!')
}
```

##### Update

Called as part of FSM flow on every frame.

```javascript
function update() {
	acc += time.dt();
}
```

##### Exit

Called as part of FSM flow.

```javascript
function exit() {
	log.info('Exit.')
}
```