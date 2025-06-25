type string: IEnumerable<char>;

static val: String;

type Test<T: IEnumerable<string> + IComparable> {
  value: Dictionary<string, T>;
  Foo<T: IEnumerable<string> + IComparable>(string, ...string[]): T;
  Bar(int): void;
  value: Bar;
}

type ThisIsAClass: IEnumerable<string>;

type Test2<T: IDK>{
  Foo(Arg1, Arg2<IEnumerable<String>>, ...string[]): T;
}