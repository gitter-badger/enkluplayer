# Setup

#### Subscribe

```
// subscribe to a message type
router.Subscribe(messageType, message => {
	...
});

// subscribe once
router.SubscribeOnce(messageType, message => {
	...
	
	// automatically unsubscribed
});

// subscribe to all
router.SubscribeAll((messageType, message) => {
	// this method will receive all messages

	...
});
```

#### Unsubscribe

```
// subscribe functions return function to unsubscribe with
var unsub = router.Subscribe(...);

...

unsub();
```

Or

```
// or use subscribe variants to pass unsubscribe function to subscriber
router.Subscribe(messageType, (unsub, message) => {
	...
	
	unsub();
});
```

#### Consume

```
router.Subscribe(messageType, message => {
	...

	// next subscriber won't get message 
	message.Consume();
});
```

#### Publishing

```
// publish a message
router.Publish(messageType, message);

// publish many messages
router.Publish(messageType, messageA, messageB, messageC);
```
