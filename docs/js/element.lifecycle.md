### Element Lifecycle Methods

##### Initialization

Code can be written outside of any function, which will be called immediately.

```javascript
var foo = 5;

// scripts will be executed _after_ all VineML scripts
var button = this.find('..btn-foo');
```

##### Start (Not Implemented)

Any logic put in `start` will be executed after all scripts have been initialized.

```javascript
function start() {
	
}
```