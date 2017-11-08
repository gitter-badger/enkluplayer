# Overview

The [Element](element.md) system comes with a simple DSL, called ɛql, which allows for quick searching. The DSL is loosely based on E4X, but only The Good Parts (tm).

### The Problem

Suppose we have an element hierarchy:

```
a
	- b
		- c
		...
				- z
```

That is, `a` is the root with a single child, `b`, which has a single child `c`, and on until `z`. Without a query system, it's going to be difficult to find what we're searching for.

```csharp
var c = a.Children[0].Children[0];
```

This is already ugly! More complicated queries, where we are looking for lists of objects that meet some criteria, become even more challenging.

### Queries

ɛql allows for queries from any element using the `Find` or `FindOne` methods.

```csharp
var b = a.FindOne("b");
```

This simply searches for immediate children with the id "b". What if we want `c`? We can simple separate our id searches with a period.

```csharp
var c = a.FindOne("b.c");
```

This is great when we know the object hierarchy, or when the object we're looking for is "close". Consider the case that we want `q`. This can be searched for using `..`.

```csharp
var q = a.FindOne("b..q");
```

The `..` will continue searching, recursively. This can even precede the query.

```csharp
var q = a.FindOne("..q");
```

The `.` and `..` can be combined.

```csharp
var r = a.FindOne("b.c..m.n..r");
```

`Find` can be used instead to return a list, though in our case each element has unique ids.

### There's More

This is great for searching by element id, but there are times where this isn't what we want. Instead, we can search by schema properties.

Let's find all elements that are currently visible.

```csharp
var visible = a.Find("..(@visible=true)");
```

Easy. These can also be combined by the `.` operator.

```csharp
var visible = a.Find("b.c.(@visible=true)");
```

The `@` operator matches a property name, and we can use the binary operators for querying:

```csharp
var big = a.Find("..(@size>100)");
var small = a.Find("..(@size<100)");
```

### Ideas

##### Combining Predicates

```csharp
var visibleAndBig = a.Find("..(@size>100 && @visible=true)");
```