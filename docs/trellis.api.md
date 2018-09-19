### Overview

Enklu Player also includes the SDK for working with Trellis, our backend. This document outlines the relevant pieces.

##### ApiController

`ApiController` is the entry point for working with Trellis. This object is configured to be created and managed by our dependency injection framework, StrangeIoC, however it may also be created directly.

Each resource on our backend typically has its own dedicated HTTP controller on the `ApiController` object. Each endpoint has its own request and response objects.

These can be used, simply enough, by calling the relevant controller method which will automatically require the relevant information.

```csharp
_api
	.EmailAuths
	.EmailSignIn(new Request
	{
		Email = username,
		Password = password
	})
	.OnSuccess(response => {
		// process response
	})
```

##### REST Endpoint Documentation

You may also consider using the Trellis REST API directly, in which case, please refer to the  [Trellis API Documentation](https://documenter.getpostman.com/view/2558443/6n8wrh3).