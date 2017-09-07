var log = (function() {
	var logLevel = 4;

	return {

		debug: function(message) {
			if (logLevel >= 4) {
				console.log("%c" + message, "background: #ADD8E6;");
			}
		},

		info: function(message) {
			if (logLevel >= 3) {
				console.log("%c" + message, "background: #FFFF94;");
			}
		},

		warn: function(message) {
			if (logLevel >= 2) {
				console.log("%c" + message, "background: #FFA500;");
			}
		},

		error: function(message) {
			if (logLevel >= 1) {
				console.log("%c" + message, "background: #FF3232;");
			}
		}
	};
})();

var bridge = (function() {

	var socket = null;
	var handlers = [];

	function handleFromSocket(message) {
		var messageType = message.messageType;
		if (!messageType) {
			log.warn("No message type found on message from socket.");
			return;
		}

		// copy in case an Off() is in a handler
		var handled = false;
		var copy = handlers.slice();
		for (var i = 0, len = copy.length; i < len; i++) {
			var handler = copy[i];
			if (handler.messageType === messageType) {
				handled = true;
				handler.callback(message.payload);
			}
		}

		if (!handled) {
			log.error("Unhandled message type : " + messageType);
		}
	}

	function sendToUnity(messageType, message) {
		SendMessage(
			"Network",
			"OnNetworkEvent",
			JSON.stringify(
				{
					messageType: messageType,
					payload: message || ""
				}));
	}

	return {

		Init: function() {
			socket = io.socket;

			if (!socket) {
				log.error("No socket available!");
				return;
			}

			socket.on('message', handleFromSocket);

			log.debug("Initialized.");
		},

		On: function(messageType) {
			// string it!
			messageType = Pointer_stringify(messageType);

			log.info("On [" + messageType + "]");

			if (!socket) {
				log.error("No socket to receive from.");
				return;
			}

			log.info("Adding handler.");

			handlers.push({
				messageType: messageType,
				callback: function(message) {
					sendToUnity(messageType, message);
				}
			});
		},

		Off: function(messageType) {
			// string it!
			messageType = Pointer_stringify(messageType);

			log.info("Off [" + messageType + "]");

			if (!socket) {
				log.error("No socket to stop receiving from.");
				return;
			}

			for (var i = handlers.length - 1; i >= 0; i--) {
				var handler = handlers[i];
				if (handler.messageType === messageType) {
					log.info("Found handler. Removing.");

					handlers.splice(i, 1);
				}
			}
		}
	};
})();

// merge
mergeInto(
	LibraryManager.library,
	bridge);