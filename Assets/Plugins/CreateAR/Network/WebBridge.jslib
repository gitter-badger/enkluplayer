// merge
mergeInto(
	LibraryManager.library,
	{
		init: function() {
			if (!window.bridge) {
				throw new Error("Bridge has not been set!");
			}

			window.bridge.send = function(messageType, message) {
					window.bridge.log.info("Sending [" + messageType + "] to Unity.");

					SendMessage(
						"Network",
						"OnNetworkEvent",
						JSON.stringify(
							{
								messageType: messageType,
								payload: message || ""
							}));
				};
		},

		ready: function() {
			window.bridge.isReady = true;

			if (window.bridge.isReadyCallback) {
				window.bridge.isReadyCallback();
			}
		},

		send: function(messageType, message) {
			window.bridge.send(messageType, message);
		}
	});