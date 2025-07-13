The root of the Sol grammar are the symbols in the type system.

For example, consider the following type program.

```st
type String;

type Object {
  ObjectMethod(String): String;
  VoidMethod();
}
```

It would generate a parser that looks something like the following.

```
Void = Object.VoidMethod Object.VoidMethodParams;
Object.VoidMethodParams = empty;

StringRule = StringType | Object.ObjectMethodRule;
Object.ObjectMethodRule = Object.ObjectMethodType Object.ObjectMethodParamsRule;
Object.ObjectMethodParamsRule = StringRule;
```

So consider parsing the following program (types are written with curly braces).

obj is a symbol in the current scope with the type Object

```
obj.ObjectMethod {Object.ObjectMethodType} "String" {String}
```
