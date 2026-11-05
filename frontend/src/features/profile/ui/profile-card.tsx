"use client";

import { playerProfileQueryOptions } from "@/src/entities/profile";
import { ErrorType, isEnvelopeError } from "@/src/shared/api/errors";
import { useLogout } from "@/src/features/logout";
import { useIsHydrated } from "@/src/shared/lib/use-is-hydrated";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { RoleBadge } from "@/src/shared/ui/role-badge";
import { Spinner } from "@/src/shared/ui/spinner";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";

export function ProfileCard() {
  const roles = useSessionStore(sessionSelectors.roles);
  const { logout, isPending } = useLogout();
  const hydrated = useIsHydrated();

  const player = useQuery(playerProfileQueryOptions());

  if (player.isLoading) {
    return <Spinner label="Загружаем профиль..." />;
  }

  // 401 → не авторизованы. Это валидное состояние, а не «ошибка».
  if (
    isEnvelopeError(player.error) &&
    player.error.type === ErrorType.AUTHENTICATION
  ) {
    return (
      <div className="space-y-3">
        <p className="text-sm text-slate-600">Вы не авторизованы.</p>
        <div className="flex gap-3">
          <Link
            href={routes.login}
            className="inline-flex h-9 items-center justify-center rounded-md bg-slate-900 px-4 text-sm font-medium text-white transition-colors hover:bg-slate-800"
          >
            Войти
          </Link>
          <Link
            href={routes.register}
            className="inline-flex h-9 items-center justify-center rounded-md border border-slate-300 px-4 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
          >
            Регистрация
          </Link>
        </div>
      </div>
    );
  }

  if (player.error) {
    return (
      <ApiErrorList error={player.error} fallback="Не удалось загрузить профиль" />
    );
  }

  if (!player.data) return null;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-4">
        <span className="inline-flex items-center gap-2 text-xs text-slate-500">
          Схема:
          <span className="rounded bg-slate-100 px-2 py-0.5 font-medium text-slate-700">
            OIDC (Bearer)
          </span>
        </span>
        <button
          type="button"
          onClick={() => void player.refetch()}
          disabled={player.isFetching}
          className="text-xs text-slate-500 underline-offset-2 hover:text-slate-900 hover:underline disabled:opacity-50"
        >
          {player.isFetching ? "Обновляем..." : "Обновить"}
        </button>
      </div>

      <section className="space-y-2">
        <h3 className="text-xs font-semibold tracking-wide text-slate-500 uppercase">
          Профиль игрока
        </h3>
        <dl className="grid grid-cols-1 gap-y-2 rounded-md border border-slate-200 px-4 py-3 text-sm sm:grid-cols-[160px_1fr]">
          <Field label="Ник" value={player.data.nickname} />
          <Field label="Регион" value={player.data.region} />
          <Field label="Создан" value={formatDate(player.data.createdAt)} />
          <Field label="Обновлён" value={formatDate(player.data.updatedAt)} />
        </dl>
      </section>

      <section className="space-y-2">
        <h3 className="text-xs font-semibold tracking-wide text-slate-500 uppercase">
          Аккаунт
        </h3>
        <dl className="grid grid-cols-1 gap-y-2 rounded-md border border-slate-200 px-4 py-3 text-sm sm:grid-cols-[160px_1fr]">
          <Field label="ID" value={player.data.id} mono />
          <Field label="Email" value={player.data.email} />
          <Field label="Username" value={player.data.userName} />
        </dl>
      </section>

      {hydrated && roles.length > 0 && (
        <section className="space-y-2">
          <h3 className="text-xs font-semibold tracking-wide text-slate-500 uppercase">
            Роли (из токена)
          </h3>
          <div className="flex flex-wrap gap-2">
            {roles.map((role) => (
              <RoleBadge key={role} role={role} />
            ))}
          </div>
        </section>
      )}

      <div className="flex items-center gap-3">
        <Link
          href={routes.home}
          className="inline-flex h-9 items-center justify-center rounded-md px-3 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
        >
          На главную
        </Link>
        <Button
          variant="ghost"
          onClick={() => void logout()}
          disabled={isPending}
        >
          {isPending ? "Выходим..." : "Выйти"}
        </Button>
      </div>
    </div>
  );
}

function Field({
  label,
  value,
  mono = false,
}: {
  label: string;
  value: string | null;
  mono?: boolean;
}) {
  return (
    <>
      <dt className="text-slate-500">{label}</dt>
      <dd
        className={`break-all ${mono ? "font-mono text-xs" : ""} ${
          value === null ? "text-slate-400 italic" : "text-slate-900"
        }`}
      >
        {value ?? "не задано"}
      </dd>
    </>
  );
}

function formatDate(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;

  return date.toLocaleString("ru-RU", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}
