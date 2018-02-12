### Overview

The intent of the `spireplayer` scripting system is to allow content creators to write scripts that are able to be run on any platform without a new build. This flexible scripting system will also help iteration speed, even without hotloading of scripts.

### Why JavaScript

To that end, we've chosen `javascript` as the language of choice for this task. There is no shortage of scripting languages that would be better to write scripts in. This fact is not lost on us. However, JavaScript has a few characteristics that make it the best fit: adoption, familiarity, and the availability of a decent C# runtime. This last fact is probably the most relevant. While scripters may be able to use some existing JavaScript code, our guess is that they won't very often. Not having to write a runtime that runs on all our target platforms, however, is really the meat and potatoes of our decision. It took under an hour to port the latest `Jint` codebase to Unity.

### Jint

[Jint](https://github.com/sebastienros/jint) is a JS parser and interpreter written entirely in C#. Jint is important because it supports ES5 (soon ES6) and does not JIT code. There were other choices, but Jint was the easiest to get running in a restricted Unity environment.

### Usage

##### ScriptManager

`IScriptManager` is the entry point into the scripting system. Scripts are preloaded using the `Asset` system, then created and managed via `IScriptManager`. Scripts are actually executed via `MonoBehaviourSpireScript`, which adds some APIs useful for working in Unity.

##### UnityScriptingHost

`UnityScriptingHost` is an extension of Jint's `Engine` class. The scripting host adds some useful APIs like logging and require. The scripting host should generally not be touched.

##### Script Logic

`Jint` can parse and interpret any ES5 compatible code. Soon, it will have ES6 support, which will allow for mostly syntactic benefits. Because of this, feel free to write logic just like you would on any JS interpreter.

##### Require

The `require` method is something common to `RequireJS` or `CommonJS` module systems, except that this one also supports C# interop very easily. Essentially, `require` allows you to include another script by id. In order to prepare a script for include, however, it needs to use the common `module.exports` method made popular by `node`.

For example, if you had a script:

```javascript
module.exports = function(fsm, id) {

	return {
		id: id,

		enter: function() {
		    log.debug("Booting...");
		},

		update: function() {
			fsm.changeState("Updating");
		},

		exit: function() {

		}
	};
};
```

This exports a factory function, sort of like a constructor in OOP. To use this, you could do this:

```javascript
var createState = require("MyState");
var state = createState(fsm, "MyState");
```

Useful JS scripts can be put in Resources, then loaded via path instead of id:

`var fsm = require("Resources/Common/StateMachine")();`

##### JsInterface

To call C# scripts from JS, use the `JsInterface` attribute on top of your class. This will automatically provide your class to JS scripts.

Eg - 

```csharp
[JsInterface("events")]
public class EventsInterface
{
	public void Foo()
	{
		// elided
	}
}
```

Now in my script:

```javascript
var events = require("events");

events.Foo();
```

Primitives may be passed back and forth without issue. Function pointers, however, are a little different.

Consider the following script:

```javscript

var events = require("events");

events.Subscribe("Foo", function(message) {

});
```

In order to support this, there are a couple of things we need in our function signature:

```csharp
void Subscribe(string type, Func<JsValue, JsValue[], JsValue> callback)
```

First, a JS function is passed to a C# as a `Func` with an odd signature. Recall, however, that in JavaScript, the `this` reference is dependent on the caller. So that first `JsValue` is simply a reference to `this`. The `JsValue[]` are the function parameters. Finally, the callback returns a `JsValue`. So to call the callback, we can do:

```csharp
void Subscribe(string type, Func<JsValue, JsValue[], JsValue> callback)
{
	callback(null, new JsValue[0]);
}
```

Unfortunately, this will make `this` inside the callback null. Also, you can't pass any parameters back. For that, you need to be able to create `JsValue`, which means you need a reference to the `Engine` that is executing the code. Fortunately, you can specify an `Engine` as the first parameter of any `JsInterface` method and it will automatically be passed to the method.

```csharp
void Subscribe(Engine engine, string type, Func<JsValue, JsValue[], JsValue> callback)
{
	// elided

	callback(
		// now this == global scope
		JsValue.FromObject(engine, engine),
		
		// passing an object back!
                new []
                {
                	JsValue.FromObject(engine, message)
                });
}
```

### Further Reading

* [Ideas](scripting.ideas.md)