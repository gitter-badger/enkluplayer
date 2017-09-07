mergeInto(LibraryManager.library, {
	Hello: function(message) {
		console.log("Hello : " + Pointer_stringify(message));
	}
});