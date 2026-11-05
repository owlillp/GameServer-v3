import { jwtDecode } from "jwt-decode";
import { create } from "zustand";
import { persist } from "zustand/middleware";

// Единый zustand-стор OIDC-сессии. Токены приходят из /connect/token
// (Authorization Code + PKCE), access-токен подставляется как Bearer.
// Хук имеет статические .getState()/.setState(), поэтому доступен и вне React
// (например, в интерсепторе axios).

export type SessionUser = {
  sub: string;
  name?: string;
  email?: string;
  roles: string[];
  expiresAt: Date | null;
};

export type OidcTokens = {
  accessToken: string;
  refreshToken: string | null;
  idToken: string | null;
  expiresAt: number | null;
};

type SessionData = {
  accessToken: string | null;
  refreshToken: string | null;
  idToken: string | null;
  expiresAt: number | null;
  user: SessionUser | null;
};

type SessionActions = {
  setTokens: (tokens: OidcTokens) => void;
  clear: () => void;
};

export type SessionStore = SessionData & SessionActions;

const initialState: SessionData = {
  accessToken: null,
  refreshToken: null,
  idToken: null,
  expiresAt: null,
  user: null,
};

export const useSessionStore = create<SessionStore>()(
  persist(
    (set) => ({
      ...initialState,
      setTokens: (tokens) =>
        set({
          accessToken: tokens.accessToken,
          refreshToken: tokens.refreshToken,
          idToken: tokens.idToken,
          expiresAt: tokens.expiresAt,
          user: decodeUser(tokens.accessToken, tokens.idToken),
        }),
      clear: () => set(initialState),
    }),
    {
      name: "gameserver-v3-session",
      partialize: (state) => ({
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        idToken: state.idToken,
        expiresAt: state.expiresAt,
      }),
      merge: (persisted, current) => {
        const p = (persisted ?? {}) as Partial<SessionData>;
        const accessToken = p.accessToken ?? null;
        const idToken = p.idToken ?? null;

        return {
          ...current,
          accessToken,
          refreshToken: p.refreshToken ?? null,
          idToken,
          expiresAt: p.expiresAt ?? null,
          user: accessToken ? decodeUser(accessToken, idToken) : null,
        };
      },
    },
  ),
);

const EMPTY_ROLES: string[] = [];

export const sessionSelectors = {
  accessToken: (s: SessionStore) => s.accessToken,
  refreshToken: (s: SessionStore) => s.refreshToken,
  user: (s: SessionStore) => s.user,
  isAuthenticated: (s: SessionStore) => s.accessToken !== null,
  roles: (s: SessionStore) => s.user?.roles ?? EMPTY_ROLES,
  displayName: (s: SessionStore) =>
    s.user?.name ?? s.user?.email ?? s.user?.sub ?? null,
  setTokens: (s: SessionStore) => s.setTokens,
  clear: (s: SessionStore) => s.clear,
} as const;

type AccessClaims = {
  sub?: string;
  name?: string;
  email?: string;
  role?: string | string[];
  roles?: string | string[];
  exp?: number;
};

type IdClaims = {
  name?: string;
  preferred_username?: string;
};

function decodeUser(accessToken: string, idToken: string | null): SessionUser | null {
  let access: AccessClaims;
  try {
    access = jwtDecode<AccessClaims>(accessToken);
  } catch {
    // Битый токен — user не определён, но accessToken оставляем: сервер
    // ответит 401 на следующий запрос.
    return null;
  }

  if (!access.sub) return null;

  // name в нашем flow уходит в id_token (profile scope), а не в access.
  let idName: string | undefined;
  if (idToken) {
    try {
      const id = jwtDecode<IdClaims>(idToken);
      idName = id.name ?? id.preferred_username;
    } catch {
      // id_token не критичен для user.
    }
  }

  return {
    sub: access.sub,
    name: access.name ?? idName,
    email: access.email,
    roles: normalizeRoles(access.role ?? access.roles),
    expiresAt: typeof access.exp === "number" ? new Date(access.exp * 1000) : null,
  };
}

function normalizeRoles(role: string | string[] | undefined): string[] {
  if (role === undefined) return [];
  return Array.isArray(role) ? role : [role];
}
