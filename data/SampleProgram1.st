type string: IEnumerable<char>;

type Test<T: IEnumerable<string> + IComparable> {
  Dictionary<string, T> value;
  T Foo<T: IEnumerable<string> + IComparable>(string, ...string[]);
}

type ThisIsAClass: IEnumerable<string>;
