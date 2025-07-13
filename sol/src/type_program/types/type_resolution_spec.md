A concrete type is a type that can't be simplified further.

It needs to support one of the following.

- Resolve Symbol
- Resolve Method
- Resolve array

Resolve method needs to take a set of types for inference.

Rules

```ts
type TypeRef = {
  name;
  generic_args;
};

type Object = {
  generic_param_names;
  members;
};

type ObjectInstance = {
  generic_args: Type[];
  object: Object;
};

type Member = Field | Method;

type Identifier = {
  name;
  type;
}

type Field = {
  identifier;
};

type Method = {
  name;
  params;
  return_type;
};

type ArrayType = {
  arity,
  nested_type
}

// Apply

apply = (type_ref, _) => type_ref;
apply = (object, generic_args: Type[]) =>
  new ObjectInstance(object, generic_args);
apply = (obj_instance, _) => obj_instance;
apply = (field, _) => throw;
apply = (method, generic_args) => apply_scope(method, create_method_scope(generic_args));

// Apply Scope
apply_scope = (type_ref, scope) => new TypeRef(name: scope.resolve_name(type_ref.name), generic_args: apply_scope(type_ref.generic_args));
apply_scope = (method, scope) => throw '';

// Into Self
into_self = (field) => field.identifier.into_self();
into_self = (identifier) => identifier.type.into_self();
info_self = (method) => method;
info_self = (type_ref) =>
  global.resolve(type_ref.name).apply(type_ref.generic_args);
  
// Resolve Sym
resolve_sym = (object, sym) => object.fields[sym].into_self();
```
