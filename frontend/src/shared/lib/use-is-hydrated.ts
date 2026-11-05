import { useSyncExternalStore } from "react";

const emptySubscribe = () => () => {};

// false на сервере, true после гидратации на клиенте — без setState в эффекте.
export function useIsHydrated(): boolean {
  return useSyncExternalStore(
    emptySubscribe,
    () => true,
    () => false,
  );
}
