"use client";

import { useIsHydrated } from "@/src/shared/lib/use-is-hydrated";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { SiteHeader } from "@/src/shared/ui/site-header";
import { KeyRound, Radio, UserRound } from "lucide-react";
import Link from "next/link";

const primaryLinkClass =
  "inline-flex h-10 items-center justify-center rounded-md bg-white px-5 text-sm font-medium text-slate-900 transition-colors hover:bg-slate-200";
const secondaryLinkClass =
  "inline-flex h-10 items-center justify-center rounded-md border border-slate-700 px-5 text-sm font-medium text-slate-200 transition-colors hover:bg-slate-900";

const services = [
  {
    icon: KeyRound,
    title: "AuthService",
    description: "OIDC: Authorization Code + PKCE, роли и permissions в токене.",
    tone: "bg-sky-50 text-sky-700",
  },
  {
    icon: UserRound,
    title: "ProfileService",
    description: "Игровой профиль игрока по защищённому API /profiles/me.",
    tone: "bg-emerald-50 text-emerald-700",
  },
  {
    icon: Radio,
    title: "RealtimeService",
    description: "SignalR-хаб /realtime для push-уведомлений игроку.",
    tone: "bg-violet-50 text-violet-700",
  },
] as const;

// Заглушка главной: каркас под будущие доменные страницы.
export default function Home() {
  const hydrated = useIsHydrated();
  const isAuthenticated = useSessionStore(sessionSelectors.isAuthenticated);
  // До гидратации состояние сессии неизвестно — рендерим «гостевой» вариант,
  // как на сервере, чтобы не было расхождения разметки.
  const showAuthenticated = hydrated && isAuthenticated;

  return (
    <>
      <SiteHeader />

      <main className="flex-1">
        <section className="border-b border-slate-200 bg-slate-950">
          <div className="mx-auto flex w-full max-w-5xl flex-col items-center px-6 py-20 text-center">
            <p className="text-xs font-semibold tracking-widest text-slate-400 uppercase">
              Игровая платформа v3
            </p>
            <h1 className="mt-3 text-4xl font-bold tracking-tight text-white sm:text-5xl">
              GameServer
            </h1>
            <p className="mt-4 max-w-2xl text-base text-slate-300">
              Каркас платформы: единый вход через AuthService, профиль игрока и
              realtime-канал. Доменные разделы появятся здесь позже.
            </p>

            <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
              {showAuthenticated ? (
                <Link href={routes.profile} className={primaryLinkClass}>
                  Мой профиль
                </Link>
              ) : (
                <>
                  <Link href={routes.login} className={primaryLinkClass}>
                    Войти
                  </Link>
                  <Link href={routes.register} className={secondaryLinkClass}>
                    Создать аккаунт
                  </Link>
                </>
              )}
            </div>
          </div>
        </section>

        <section className="mx-auto w-full max-w-5xl px-6 py-14">
          <div className="grid gap-4 sm:grid-cols-3">
            {services.map((service) => (
              <section
                key={service.title}
                className="rounded-lg border border-slate-200 bg-white p-5"
              >
                <span
                  aria-hidden="true"
                  className={`inline-flex rounded-md p-2 ${service.tone}`}
                >
                  <service.icon className="h-5 w-5" />
                </span>
                <h2 className="mt-3 text-sm font-semibold text-slate-900">
                  {service.title}
                </h2>
                <p className="mt-1 text-sm text-slate-600">
                  {service.description}
                </p>
              </section>
            ))}
          </div>
        </section>
      </main>
    </>
  );
}
