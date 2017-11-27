#### Usage

Every schema definition follows the form:

```
QUERY {
  KEY-VALUE PAIRS
}
```

Let's look at an example.

```
myElement {
  foo: "pfffft";
  bar: 5;
  foobar: 5.1f;
  fizz: true;
  buzz: {
    "x": 0, "y": 0, "z": 0
  }
}
```

Values can be of type `string`, `int`, `float`, `boolean`, or `json`. The `json` type allows custom types to be serialized.

QUERY applies to any legal schema query.

```
..apple..fish.* {
  KEY-VALUE PAIRS
}
```

This applies to all elements the query matches.

The `KEY-VALUE PAIRS` section may be used similar to classes:

```
a {
  foo: 5;
}

b @extends a {
  bar: "five"
}
```

In the above example, all elements matching `b` will have both `foo` and `bar` properties.



#### Further Reading

* https://css-tricks.com/multiple-class-id-selectors/
* https://css-tricks.com/the-extend-concept/