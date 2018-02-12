### Overview

`IQueryResolver` is a simple description for an object that can determine whether or not a query matches a set of tags. The `StandardQueryResolver` class allows for boolean operations based on the query string.

From the source:

```csharp
// Commas are ANDs, spaces are ORs, !s are NOTs.
// 
// Egs-
// 
// "a" will resolve true for a set of tags including "a"
// "a b" will resolve true for a set of tags including "a" OR "b"
// "a,b" will resolve true for a set of tags including "a" AND "b"
// "!a" will resolve true for a set of tags NOT including "a"
// 
// These can be composed for powerful effect:
// 
// "a b,!n,z !!b" is equivalent to: "(a || b) && !n && (z || b)"
```