interface Useable {
  free(): void;
}

/** Borrow the value passed in and frees it when the function is done executing. Returns the values of the function, if there is one. */
export function using<T extends Useable, U = void>(
  value: T,
  func: (value: T) => U
) {
  const output = func(value);
  value.free();
  return output;
}
