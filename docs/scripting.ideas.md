### Runtime Modifications

The problem is that Jint is slow. It's an interpreter, so it can't JIT, and it's in C#, so it THRASHES the GC. Idea: provide a bridge to a JS runtime external to Unity runtime.

##### iOS

Use [JavaScriptCore](https://developer.apple.com/documentation/javascriptcore) to execute JS.

##### Android/HoloLens/Pi (or other ARM devices)

Use [V8](https://android.googlesource.com/platform/external/v8) to execute JS.
