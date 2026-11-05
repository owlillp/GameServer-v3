// returnUrl приходит от AuthService: неавторизованный /connect/authorize
// редиректит браузер на страницу входа фронтенда с этим параметром.
// После логина браузер должен вернуться на исходный authorize URL — это нужно
// и SPA (PKCE уже сохранён в sessionStorage), и внешним клиентам (Unity).
//
// Открытый редирект запрещён: принимаем только URL того же origin, что и API,
// и только на /connect/*.
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "";

export function sanitizeAuthReturnUrl(
  value: string | null | undefined,
): string | null {
  if (!value || !API_BASE_URL) {
    return null;
  }

  try {
    const candidate = new URL(value);
    const apiOrigin = new URL(API_BASE_URL).origin;

    if (candidate.origin !== apiOrigin) {
      return null;
    }

    if (!candidate.pathname.startsWith("/connect/")) {
      return null;
    }

    return candidate.toString();
  } catch {
    return null;
  }
}
