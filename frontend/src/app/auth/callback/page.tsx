"use client";

import {
  clearPkceState,
  exchangeCode,
  readPkceState,
} from "@/src/shared/auth/oidc";
import { resolveHomeRoute } from "@/src/shared/auth/roles";
import { routes } from "@/src/shared/routes";
import { useSessionStore } from "@/src/shared/stores/session-store";
import { Spinner } from "@/src/shared/ui/spinner";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";

// OAuth callback: меняет authorization code на токены (PKCE) и уводит дальше.
export default function AuthCallbackPage() {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    const run = async () => {
      const params = new URLSearchParams(window.location.search);
      const errorParam = params.get("error");
      const errorDescription = params.get("error_description");
      const code = params.get("code");
      const state = params.get("state");
      const saved = readPkceState();

      if (errorParam) {
        setError(errorDescription ?? `Ошибка авторизации: ${errorParam}`);
        return;
      }

      if (!code || !state || !saved || state !== saved.state) {
        setError("Некорректный ответ авторизации.");
        return;
      }

      try {
        const tokens = await exchangeCode(code, saved.verifier);
        clearPkceState();
        useSessionStore.getState().setTokens(tokens);

        // returnTo приходит только из внутренних вызовов startLogin;
        // иначе — на домашний маршрут после входа.
        router.replace(saved.returnTo || resolveHomeRoute());
      } catch (cause: unknown) {
        setError(
          cause instanceof Error ? cause.message : "Не удалось обменять код.",
        );
      }
    };

    void run();
  }, [router]);

  if (error) {
    return (
      <main className="mx-auto w-full max-w-md space-y-4 px-6 py-10">
        <h1 className="text-2xl font-semibold text-slate-900">
          Вход не удался
        </h1>
        <p className="text-sm text-red-600">{error}</p>
        <Link
          href={routes.login}
          className="inline-flex h-9 items-center justify-center rounded-md bg-slate-900 px-4 text-sm font-medium text-white"
        >
          Назад ко входу
        </Link>
      </main>
    );
  }

  return (
    <main className="flex flex-1 items-center justify-center p-10">
      <Spinner label="Завершаем вход..." />
    </main>
  );
}
