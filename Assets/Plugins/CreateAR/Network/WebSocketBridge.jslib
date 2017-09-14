// merge
mergeInto(
	LibraryManager.library,
	{
		Init: function() {
			if (!window.bridge) {
				window.bridge = {};
			}

			window.bridge.isReady = false;
			window.bridge.handlers = [];
			window.bridge.logLevel = 4;
			window.bridge.log = {
					debug: function(message) {
						if (window.bridge.logLevel >= 4) {
							console.log("%c" + message, "background: #ADD8E6;");
						}
					},

					info: function(message) {
						if (window.bridge.logLevel >= 3) {
							console.log("%c" + message, "background: #FFFF94;");
						}
					},

					warn: function(message) {
						if (window.bridge.logLevel >= 2) {
							console.log("%c" + message, "background: #FFA500;");
						}
					},

					error: function(message) {
						if (window.bridge.logLevel >= 1) {
							console.log("%c" + message, "background: #FF3232;");
						}
					}
				};

			window.bridge.handleFromSocket = function (message) {
					var messageType = message.messageType;
					if (!messageType) {
						return;
					}

					// copy in case an Off() is in a handler
					var handled = false;
					var copy = window.bridge.handlers.slice();
					for (var i = 0, len = copy.length; i < len; i++) {
						var handler = copy[i];
						if (handler.messageType === messageType) {
							handled = true;
							handler.callback(message.payload);
						}
					}
				};

			window.bridge.sendToUnity = function(messageType, message) {
					window.bridge.log.info("Sending [" + messageType + "] to Unity.");

					SendMessage(
						"Network",
						"OnNetworkEvent",
						messageType + ";" + JSON.stringify(message || {}));
				};

			if (!window.bridge.socket) {
				window.bridge.log.error("No socket available!");
				return;
			}

			window.bridge.socket.on('message', window.bridge.handleFromSocket);
			window.bridge.log.debug("Initialized.");
		},

		Ready: function() {
			window.bridge.isReady = true;
			if (window.bridge.isReadyCallback) {
				window.bridge.isReadyCallback();
			}
		},

		On: function(messageType) {
			// string it!
			messageType = Pointer_stringify(messageType);

			window.bridge.log.info("On [" + messageType + "]");

			if (!window.bridge.socket) {
				window.bridge.log.error("No socket to receive from.");
				return;
			}

			window.bridge.log.info("Adding handler.");

			window.bridge.handlers.push({
				messageType: messageType,
				callback: function(message) {
					window.bridge.sendToUnity(messageType, message);
				}
			});
		},

		Off: function(messageType) {
			// string it!
			messageType = Pointer_stringify(messageType);

			window.bridge.log.info("Off [" + messageType + "]");

			if (!window.bridge.socket) {
				window.bridge.log.error("No socket to stop receiving from.");
				return;
			}

			for (var i = window.bridge.handlers.length - 1; i >= 0; i--) {
				var handler = window.bridge.handlers[i];
				if (handler.messageType === messageType) {
					window.bridge.log.info("Found handler. Removing.");

					window.bridge.handlers.splice(i, 1);
				}
			}
		},

		SendToUnity: function(messageType, message) {
			window.bridge.sendToUnity(messageType, message);
		}
	});