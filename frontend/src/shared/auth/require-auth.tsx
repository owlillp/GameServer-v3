"use client";

import { useIsHydrated } from "@/src/shared/lib/use-is-hydrated";
import { routes } from "@/src/shared/routes";
import {
  sessionSelectors,
  useSessionStore,
} from "@/src/shared/stores/session-store";
import { PageLoader } from "@/src/shared/ui/page-loader";
import { useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { hasAnyRole, resolveHomeRoute } from "./roles";

type Props = {
  children: ReactNode;
  // Если роли не заданы — достаточно быть аутентифицированным.
  roles?: readonly string[];
};

// Клиентский guard: до гидратации показываем загрузку (в localStorage ещё нет
// сессии), затем либо рендерим контент, либо уводим на вход/домашний маршрут.
export function RequireAuth({ children, roles }: Props) {
  const router = useRouter();
  const hydrated = useIsHydrated();
  const isAuthenticated = useSessionStore(sessionSelectors.isAuthenticated);
  const user = useSessionStore(sessionSelectors.user);

  const allowed = !roles || hasAnyRole(user?.roles ?? [], roles);
  const hasAccess = isAuthenticated && allowed;

  useEffect(() => {
    if (!hydrated) return;

    const userRoles = user?.roles ?? [];

    if (!isAuthenticated) {
      router.replace(routes.login);
      return;
    }

    if (roles && !hasAnyRole(userRoles, roles)) {
      router.replace(resolveHomeRoute());
    }
  }, [hydrated, isAuthenticated, roles, router, user]);

  if (!hydrated || !hasAccess) {
    return <PageLoader />;
  }

  return <>{children}</>;
}
