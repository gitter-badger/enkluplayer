// merge
mergeInto(
	LibraryManager.library,
	{
		/**
		 * Called on Awake(), i.e. essentially as early as possible.
		 */
		init: function() {
			// The react project should have added a bridge!
			if (!window.bridge) {
				throw new Error("Bridge has not been set! This is probably an error with the spire-react project.");
			}

			window.bridge.log.info("WebBridge.jslib::init()");

			// add method to bridge for sending events to Unity
			window.bridge.send = function(message) {
					window.bridge.log.info("Sending [" + message + "] to Unity.");
					
					SendMessage(
						"Network",
						"OnNetworkEvent",
						message);
				};
		},

		/**
		 * Called when Unity is ready.
		 */
		ready: function() {
			window.bridge.log.info("WebBridge.jslib::ready()");

			window.bridge._isReadyCallback();
		}
	});