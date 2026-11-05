"use client";

import { useLogout } from "@/src/features/logout";
import { isStaff } from "@/src/shared/auth/roles";
import { cn } from "@/src/shared/lib/utils";
import { useIsHydrated } from "@/src/shared/lib/use-is-hydrated";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { Button } from "@/src/shared/ui/button";
import { RoleBadge } from "@/src/shared/ui/role-badge";
import { Gamepad2 } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

type NavItem = {
  href: string;
  label: string;
};

export function SiteHeader() {
  const pathname = usePathname();
  const hydrated = useIsHydrated();
  const storedUser = useSessionStore(sessionSelectors.user);
  const storedAuth = useSessionStore(sessionSelectors.isAuthenticated);
  const { logout, isPending } = useLogout();

  // До гидратации не знаем состояние сессии — рендерим нейтральный вариант,
  // совпадающий с серверной разметкой.
  const isAuthenticated = hydrated && storedAuth;
  const user = hydrated ? storedUser : null;
  const userRoles = user?.roles ?? [];
  const staff = isStaff(userRoles);

  const navItems: NavItem[] = isAuthenticated
    ? [{ href: routes.profile, label: "Профиль" }]
    : [];

  return (
    <header className="sticky top-0 z-20 border-b border-slate-200 bg-white/85 backdrop-blur">
      <div className="mx-auto flex w-full max-w-5xl flex-wrap items-center gap-x-6 gap-y-2 px-6 py-3">
        <Link
          href={routes.home}
          className="flex items-center gap-2 text-sm font-semibold tracking-tight text-slate-900"
        >
          <span
            aria-hidden="true"
            className="inline-flex h-7 w-7 items-center justify-center rounded-lg bg-slate-900 text-white"
          >
            <Gamepad2 className="h-4 w-4" />
          </span>
          GameServer
          <span className="rounded bg-slate-100 px-1.5 py-0.5 text-[10px] font-bold tracking-widest text-slate-500">
            V3
          </span>
        </Link>

        {navItems.length > 0 && (
          <nav aria-label="Основная навигация" className="flex items-center">
            {navItems.map((item) => {
              const active =
                pathname === item.href || pathname.startsWith(`${item.href}/`);

              return (
                <Link
                  key={item.href}
                  href={item.href}
                  aria-current={active ? "page" : undefined}
                  className={cn(
                    "rounded-md px-3 py-1.5 text-sm transition-colors",
                    active
                      ? "bg-slate-100 font-medium text-slate-900"
                      : "text-slate-600 hover:bg-slate-50 hover:text-slate-900",
                  )}
                >
                  {item.label}
                </Link>
              );
            })}
          </nav>
        )}

        <div className="ml-auto flex items-center gap-2">
          {isAuthenticated ? (
            <>
              <span className="hidden items-center gap-2 text-xs text-slate-500 sm:inline-flex">
                {user?.name ?? user?.email ?? user?.sub}
                {staff && <RoleBadge role={userRoles[0] ?? "Admin"} />}
              </span>
              <Button
                variant="ghost"
                onClick={() => void logout()}
                disabled={isPending}
              >
                {isPending ? "Выходим..." : "Выйти"}
              </Button>
            </>
          ) : (
            <>
              <Link
                href={routes.login}
                className="inline-flex h-9 items-center justify-center rounded-md px-3 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
              >
                Войти
              </Link>
              <Link
                href={routes.register}
                className="inline-flex h-9 items-center justify-center rounded-md bg-slate-900 px-4 text-sm font-medium text-white transition-colors hover:bg-slate-800"
              >
                Регистрация
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}
