### Client (C#)

##### Create and sync a transaction.

```csharp
// create a new transaction
var req = new ElementTransaction()
    .Create(data[0])
    .Update(data[1])
    .Update(data[2]);

// performs txn and pushes to server
var record = _txns.Sync(req);

println(record.TxnId);		// unique id
println(record.State);		// InProgress

// listen for commit
record
	.Token
    .OnSuccess(r => {
    	println(r.State);	// Committed | RolledBack
    });
```

##### Receive a transaction.

```csharp
// only committed transactions are received
_txns.OnReceived += txn => {
  	//   
};
```



### Client (JS)

##### Create and sync a transaction.

```javascript
// create a new transaction
var txn = transactions
	.create(data)
	.update(data);

// synchronize it
var record = transactions.sync(txn);

// listen for successes
record.OnSuccess(function(r) {
    // 
});
```

##### Receive a transaction.

```javascript
// only commited transactions are received
transactions.OnReceived(function(txn) {
    // 
});
```



### Server

##### All transactions pass serially through `ElementTransactionService`.

```javascript
ElementService.apply(
    txn,
    function(result) {
        if (result.state === "Committed") {
            // txn has been committed
        } else if (result.state === "RolledBack") {
            // txn has been rolled back
        }
    });
```

