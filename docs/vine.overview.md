### Overview

If the `Element` system is the DOM and `Schema` is styling, then `VineML` is HTML. `VineML` is the language for describing element hierarchies and associating schema data with them. An authored vine file is called, quite simple, a _vine_. 

#### Usage

It's easiest to start by example.

```html
<?Vine>

<Caption label='Hello World'/>
```

Vines should start with the header `<?Vine>`. A specific version may be included as a property, otherwise the latest version will be used.

Vines may only contain one root element. The following vine will fail to parse:

```html
<?Vine>

<Caption label='A' />
<Caption label='B' />
```

Element tags work similarly to XHTML, except that _any_ element may use either standard or self closing tags.

Properties may be added to element tags, like the `label` property above. These properties will be injected into the element's schema. There are several types of supported primitives: `int`, `float`, `string`, `bool`, `Vec3`, and `Col4`.

```html
<?Vine>

<Caption
	visible=true
	label='A'
	fontSize=80
	position=(0, 10, 0)
	color=#FF0000 />
```

