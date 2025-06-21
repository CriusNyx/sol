type string: IEnumerable<char>;

type Test<T: IEnumerable<string> + IComparable> {
  Dictionary<string, T> value;
  T Foo<T: IEnumerable<string> + IComparable>(string, ...string[]);
  void Bar(int);
  Baz value;
}

type ThisIsAClass: IEnumerable<string>;

type Test2<T: IDK>{
  T Foo(Arg1, Arg2<IEnumerable<String>>, ...string[]);
}