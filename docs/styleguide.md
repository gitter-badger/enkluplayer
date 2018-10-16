### Overview

To ensure the highest quality products, we must ensure the highest quality code. This document defines the standard for every line of C# committed to this repository: tools, tests, debug code, etc. The only exception is 3rd party plugins, which may not abide by this standard.

### Files

##### Formatting

* Files should have a soft limit of 80 columns and a hard limit of 100 columns
* The soft limit is what should generally be followed.
	* The hard limit is a rule.
	* Use the **Editor Guidelines Extension** to help manage this.
* Indentation should **use 4 spaces**, not tabs.
* **Unix style line endings**.

##### Directory Structure

* **Organize .cs files**. Make sure they are in appropriate folders, not in the project root.
* **One public class per file**. Multiple internal classes are permissible.

##### Usings

* All using statements should be at the top of the file.
* Use Resharper to clean + optimize using statements. All unused using statements should be removed.
* When using conditional compilation, avoid leaning on using statements. This is explained in more detail in the *Conditional Compilation* section.

##### Namespaces

* We want to find a good balance between the advantages of namespaces and the overhead of too many namespaces.
* Namespace usage should be fairly minimal and follow this convention: `CreateAR.EnkluPlayer.[System]`. The **System** portion is optional.
* All namespaces begin with CreateAR, to disambiguate our code with other parties.

### Classes

##### Member Order

Class members are required to use the following order:

* Internal type definitions
	* Enums
	* Classes
	* Delegate definitions
* Serialized fields (in the case of a MonoBehaviour)
* Public fields
* Protected fields
* Private fields
* Injected Properties
* Properties
* Events
* Public methods
* Protected methods
* Private methods

Methods should always be ordered:

* Static
* Virtual instance
* Instance

##### Comments

* We use XmlDoc format.
* Every class should have a comment.
	* This comment should describe what the object is for.
	* Document usage here or provide a link to usage documentation.
* Every member  should be commented: fields, properties, methods, events, etc.
	* For commonly used dependencies, you may comment in this way:
```
/// <summary>
/// Dependencies.
/// </summary>
private readonly IFoo _foo;
private readonly IBar _bar;
private readonly IZoo _zoo;
```

* For simple constructors, you may leave off parameters.
* **No cursing, complaining, or being mean**. It is completely inappropriate and unprofessional. Do not entomb a bad attitude in our code base.
* **No crude jokes.** It doesn't need to be said.
* **Good puns are much appreciated.**

##### Internal Types

* **Entire standard applies**.
* If an internal type becomes to large or used externally, extract to new file.

##### Serialized Variables

* This applies to **MonoBehaviours or classes marked with [Serializable].**
* **Use [Tooltip]** to provide useful comments.
* **Use [Range]** on primitives for data validation.

##### Private and Protected Variables

* These use **\_camelCase**.
* Use **short, descriptive names**.
	* `projectileManager` is a longer name than `projectiles`, but conveys no new information.

Consider a property on the class *ProjectileManager*.

`IReadOnlyCollection<Projectile> Projectiles { get; }`

This results in callers using something like:

`var projectiles = _projectiles.Projectiles;`

Instead, consider this shorter version that is just as descriptive:

`var projectiles = _projectiles.All;`

##### Constants

* Constants should not have any special prefix.
* The should be written with **ALL_CAPS_AND_UNDERSCORES.**

##### Public Variables

* Capital **CamelCase**.
* **Note: no underscore**.

##### Delegate Definitions

* **Prefer Action and Func.**
* If a real need is determined, follow conventions for public and non-public methods.

##### Events

* Same as variables: dependent on access level.
* Should begin with "On" .
	* Debatable.
	* Note: **Microsoft's event naming guidelines** specifically prohibit this. But we think they are wrong.
* Name should follow the convention `[Source]_[Name of event]`. Eg - `_projectiles.OnFired += Projectiles_OnFired;`

##### Methods

* **CapitalCamelCase.**
* Follow conventions for variables of the same access level.
* Do not prefix a method name with "Do" or "Actually" or similarly silly non-descriptor.
	* Take a walk and think up something more descriptive.
	* Having trouble? Ask someone else.

##### Braces

* Braces should be on their own line.
	* The one exception: auto-properties.
* **Always use braces**.

Q: What if I have something like:

`if (foo) bar = 1;`

A: No. Always use braces.

* **Braces are not optional**.

Q: What about in this case?

```
using (var foo = new Foo())
using (var bar = new Bar())
{
    //
}
```

A: No. Braces are not optional.

* **Do not skip braces.**

Q: Can I skip braces for things like?

`if (foo) return;`

A: No. Do not skip braces.

* **Lose braces break vases.**

Q: Can I leave out the braces in simple case statements?

```
switch (type)
{
    case 1: return "Foo";
    default: return "Bar";
}
```

A: No. Braces are not optional.

### Logic

##### Line Breaks and Spacing

* **Always use spaces** between operators.

`var a = 1 + 2 - 3;`

* **Remove unnecessary parenthesis**.

`var b = (foo && bar) || zoop;`

* **Break up long statements** along the operators.

```
if (IsVisible()
	&& IsEnabled()
	&& (foo && bar)
	&& IsThisTooLong)
{
	//
}
```

* **Break up method/member chains** along the dot operator.

```
_builder
	.WithFoo(...)
	.WithBar(...)
	.Select(...);
```

* **Use line breaks with excessive method parameters.**

```
_system.Foo(
	bar,
	new Vector3(1, 2, 3),
	_instance,
	_other);
```

* **Use line breaks with closures that require braces**.

```
() =>
{
	// 
}
```

##### Functions

* Use  short, concise  functions _when you can_.
* If a function is over 20 lines long, consider splitting it into smaller functions.
* Avoid the [**boolean trap**](https://ariya.io/2011/08/hall-of-api-shame-boolean-trap). There are many strategies in the article to deal with this.
* If a function takes over four parameters, consider using a parameter object instead.

##### Loops

* **Avoid while(true) loops**. Whenever possible, refactor into more predictable conditional loops.
* User for loops instead of foreach loops wherever posible.
* **Cache length in for loops**.

##### Misc

* **Do not submit code with warnings**.
* **Prefer var** unless readability is severely impacted.
* If your method is long, break it into multiple methods.
* If your class is long, break it into multiple classes.