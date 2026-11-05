// OAuth 2.0 Authorization Code + PKCE клиент для SPA (без BFF).
// Логин: /auth/login ставит Identity-cookie → затем редирект на /connect/authorize.
// Обмен кода и refresh идут на /connect/token.

import type { OidcTokens } from "@/src/shared/stores/session-store";

const AUTHORITY = process.env.NEXT_PUBLIC_API_URL ?? "";
const CLIENT_ID = process.env.NEXT_PUBLIC_OIDC_CLIENT_ID ?? "gameserver-v3-web";
const SCOPE =
  "openid profile email offline_access auth profiles wallet catalog inventory store events realtime";
const STORAGE_KEY = "gameserver-v3-oidc";

type PkceState = {
  verifier: string;
  state: string;
  returnTo: string;
};

export const redirectUri = (): string =>
  `${window.location.origin}/auth/callback`;

function base64Url(bytes: Uint8Array): string {
  let binary = "";
  for (const byte of bytes) {
    binary += String.fromCharCode(byte);
  }

  return btoa(binary)
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
}

function randomUrlSafe(bytes = 32): string {
  const buffer = new Uint8Array(bytes);
  crypto.getRandomValues(buffer);
  return base64Url(buffer);
}

async function s256(verifier: string): Promise<string> {
  const digest = await crypto.subtle.digest(
    "SHA-256",
    new TextEncoder().encode(verifier),
  );

  return base64Url(new Uint8Array(digest));
}

// Строит authorize URL, сохраняет verifier/state и уводит браузер.
// returnTo — необязательный маршрут после входа; пусто → role-based домашний.
export async function startLogin(returnTo = ""): Promise<void> {
  const verifier = randomUrlSafe(32);
  const state = randomUrlSafe(16);
  const challenge = await s256(verifier);

  const pkce: PkceState = { verifier, state, returnTo };
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(pkce));

  const url = new URL(`${AUTHORITY}/connect/authorize`);
  url.searchParams.set("response_type", "code");
  url.searchParams.set("client_id", CLIENT_ID);
  url.searchParams.set("redirect_uri", redirectUri());
  url.searchParams.set("scope", SCOPE);
  url.searchParams.set("code_challenge", challenge);
  url.searchParams.set("code_challenge_method", "S256");
  url.searchParams.set("state", state);

  window.location.assign(url.toString());
}

export function readPkceState(): PkceState | null {
  const raw = sessionStorage.getItem(STORAGE_KEY);
  if (!raw) return null;

  try {
    return JSON.parse(raw) as PkceState;
  } catch {
    return null;
  }
}

export function clearPkceState(): void {
  sessionStorage.removeItem(STORAGE_KEY);
}

export async function exchangeCode(
  code: string,
  verifier: string,
): Promise<OidcTokens> {
  return postToken(
    new URLSearchParams({
      grant_type: "authorization_code",
      code,
      redirect_uri: redirectUri(),
      client_id: CLIENT_ID,
      code_verifier: verifier,
    }),
  );
}

export async function refreshTokens(refreshToken: string): Promise<OidcTokens> {
  return postToken(
    new URLSearchParams({
      grant_type: "refresh_token",
      refresh_token: refreshToken,
      client_id: CLIENT_ID,
    }),
  );
}

// Отзывает refresh/access токен на стандартном OAuth revocation endpoint.
// Public-клиент аутентифицируется одним client_id (без secret).
export async function revokeToken(token: string): Promise<void> {
  await fetch(`${AUTHORITY}/connect/revoke`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body: new URLSearchParams({ token, client_id: CLIENT_ID }),
  });
}

async function postToken(body: URLSearchParams): Promise<OidcTokens> {
  const response = await fetch(`${AUTHORITY}/connect/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
  });

  if (!response.ok) {
    throw new Error(`Token request failed: ${response.status}`);
  }

  const json = (await response.json()) as {
    access_token: string;
    refresh_token?: string;
    id_token?: string;
    expires_in?: number;
  };

  return {
    accessToken: json.access_token,
    refreshToken: json.refresh_token ?? null,
    idToken: json.id_token ?? null,
    expiresAt: json.expires_in ? Date.now() + json.expires_in * 1000 : null,
  };
}
