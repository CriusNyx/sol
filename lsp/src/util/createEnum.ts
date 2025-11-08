type ObjectEnum<T extends readonly string[]> = Record<T[number], T>;

export function createEnum<T extends readonly string[]>(
  ...source: T
): ObjectEnum<T> {
  return source.reduce(
    (prev, current) => ({ ...prev, [current]: current }),
    {},
  ) as ObjectEnum<T>;
}
