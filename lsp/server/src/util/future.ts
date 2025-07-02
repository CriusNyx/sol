export function createFuture<T>() {
  let setFuture: (value: T) => void = undefined as any;
  const future = new Promise<T>((res) => {
    setFuture = res;
  });
  return [future, setFuture] as const;
}
