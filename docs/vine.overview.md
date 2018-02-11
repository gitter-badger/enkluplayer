### Overview

If the `Element` system is the DOM and `Schema` is styling, then `VineML` is HTML. `VineML` is the language for describing element hierarchies and associating schema data with them. A `VineML` file is called, quite simply, a _vine_. 

#### Basics

It's easiest to start by example.

```html
<?Vine version='0.1.0'>

<Container>
	<Caption label='Hello World'/>
</Container>
```

Vines may start with the header `<?Vine>`. A specific version may be included as a property. If the header is missing or has no version, the latest version will be assumed.

Vines may only contain one root element:

```html
<Caption label='A' />
<Caption label='B' /> // parse error!
```

Element tags work similarly to XHTML, except that _any_ element may use either standard or self closing tags.

```html
<Container>
	<Caption label='A' />
  	<Caption label='A'></Caption>
</Container>
```

Unlike HTML, elements can only contain other elements, not raw text.

```html
<Caption>This is invalid!</Caption> // parser error
```

Properties may be added to element tags, like the `label` property above. These properties will be added to the element's schema. There are several types of supported primitives: `int`, `float`, `string`, `bool`, `Vec3`, and `Col4`-- the latter two have literals not found in HTML.

```html
<Caption
	visible=true
	label='A'
	fontSize=80
	position=(0, 10, 0)
	color=#FF0000 />
```

Note that the string literal accepts _only single quotes_. Double quotes are not valid delimiters.

Property names may have dashes or dots. This is often used to act like property objects.

```html
<Menu header.width=100 header.fontSize=40 />
```

Finally, `VineML` supports C-style comments, not HTML style.

```html
<Container>
  	// This is the scan menu
	<Menu ... />
  	
  	/**
  	 * These elements are here for...
  	 */
  	<Caption ... />
  	<Image ... />
</Container>
```

### JS Preprocessor

The default `VineML` importer, `VineImporter`, comes with a simple preprocessor that allows blocks of JS to be executed before the vine is parsed. Any string these blocks return are injected into the vine.

```html
<Container>
  {{
  	var counter = 5;
  	var acc = "";
    while (--counter > 0) {
  		acc += "<Button label='" + counter + "' />";
    }
  	
  	retur acc;
  }}
</Container>
```
### Further Reading

* [Writing Controllers](vine.controller.md)