import { routes } from "@/src/shared/routes";

// Роли зеркалят AuthService.Domain.AuthRoles и claim "role" в access-токене.
export const Role = {
  USER: "User",
  MODERATOR: "Moderator",
  ADMIN: "Admin",
  SERVICE: "Service",
} as const;

export type RoleName = (typeof Role)[keyof typeof Role];

// Кто получает доступ к админ-панели (permission users.view на бэкенде).
export const STAFF_ROLES: readonly string[] = [Role.ADMIN, Role.MODERATOR];

export function hasAnyRole(
  userRoles: readonly string[],
  required: readonly string[],
): boolean {
  return required.some((requiredRole) =>
    userRoles.some(
      (userRole) => userRole.toLowerCase() === requiredRole.toLowerCase(),
    ),
  );
}

export function isStaff(userRoles: readonly string[]): boolean {
  return hasAnyRole(userRoles, STAFF_ROLES);
}

// Домашний маршрут после входа. Пока у всех ролей один — профиль; когда появится
// админ-панель, персонал (isStaff) можно уводить туда.
export function resolveHomeRoute(): string {
  return routes.profile;
}
