import { authApi } from "@/src/entities/auth";
import { startLogin } from "@/src/shared/auth/oidc";
import { useMutation } from "@tanstack/react-query";

type UseLoginOptions = {
  // Внутренний маршрут после OIDC-редиректа; пусто → role-based домашний.
  returnTo?: string;
  // Внешний authorize URL (например, из Unity-флоу) — после логина уводим
  // браузер прямо на него.
  returnUrl?: string | null;
};

// Логин: /auth/login ставит Identity-cookie, затем либо возвращаемся на
// исходный authorize URL, либо уходим в стандартный OIDC-флоу приложения.
export function useLogin({ returnTo = "", returnUrl }: UseLoginOptions = {}) {
  const mutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: async () => {
      if (returnUrl) {
        window.location.assign(returnUrl);
        return;
      }

      await startLogin(returnTo);
    },
  });

  return {
    login: mutation.mutateAsync,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
