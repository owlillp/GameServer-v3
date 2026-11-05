"use client";

import { revokeToken } from "@/src/shared/auth/oidc";
import { routes } from "@/src/shared/routes";
import { useSessionStore } from "@/src/shared/stores/session-store";
import { useRouter } from "next/navigation";
import { useCallback, useState } from "react";

// Выход: отзываем refresh-токен на /connect/revoke, чистим локальную сессию
// и уходим на вход. Даже если отзыв не удался — локально выходим.
export function useLogout() {
  const router = useRouter();
  const [isPending, setIsPending] = useState(false);

  const logout = useCallback(async () => {
    setIsPending(true);

    const refreshToken = useSessionStore.getState().refreshToken;
    if (refreshToken) {
      try {
        await revokeToken(refreshToken);
      } catch {
        // Игнорируем сетевые ошибки — сессия всё равно очищается.
      }
    }

    useSessionStore.getState().clear();
    router.replace(routes.login);
  }, [router]);

  return { logout, isPending };
}
