import { refreshTokens } from "@/src/shared/auth/oidc";
import { useSessionStore } from "@/src/shared/stores/session-store";
import axios, { AxiosHeaders, type InternalAxiosRequestConfig } from "axios";
import { Envelope } from "./envelope";
import { EnvelopeError, ErrorType } from "./errors";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

declare module "axios" {
  export interface AxiosRequestConfig {
    // Не пытаться обновлять access-токен при 401 (login/register/token).
    skipAuthRefresh?: boolean;
    // Внутренний флаг: запрос уже повторяли после refresh.
    _retry?: boolean;
  }

  export interface InternalAxiosRequestConfig {
    skipAuthRefresh?: boolean;
    _retry?: boolean;
  }
}

export const apiClient = axios.create({
  baseURL: BASE_URL,
  // Без withCredentials axios не отправляет Identity-cookie (нужна для /auth/login).
  withCredentials: true,
  headers: { "Content-Type": "application/json" },
});

// Дедупликация: параллельные 401 ждут один и тот же refresh.
let refreshPromise: Promise<string> | null = null;

// Если есть access-токен — добавляем Authorization: Bearer.
apiClient.interceptors.request.use((config) => {
  const token = useSessionStore.getState().accessToken;

  if (token) {
    config.headers = AxiosHeaders.from(config.headers);
    config.headers.set("Authorization", `Bearer ${token}`);
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => {
    const envelope = response.data as Envelope;

    if (envelope?.isError && envelope.error) {
      return Promise.reject(new EnvelopeError(envelope.error));
    }

    return response;
  },
  async (error) => {
    if (axios.isAxiosError(error)) {
      const config = error.config as InternalAxiosRequestConfig | undefined;

      // 401 -> один раз пробуем обновить access-токен и повторить запрос.
      if (config && shouldRefresh(error.response?.status, config)) {
        config._retry = true;
        try {
          const token = await getRefreshPromise();
          config.headers = AxiosHeaders.from(config.headers);
          config.headers.set("Authorization", `Bearer ${token}`);
          return apiClient(config);
        } catch (refreshError) {
          useSessionStore.getState().clear();
          return Promise.reject(refreshError);
        }
      }

      if (error.response?.data) {
        const envelope = error.response.data as Envelope;

        if (envelope?.isError && envelope.error) {
          return Promise.reject(new EnvelopeError(envelope.error));
        }
      }
    }

    return Promise.reject(error);
  },
);

function shouldRefresh(
  status: number | undefined,
  config: { skipAuthRefresh?: boolean; _retry?: boolean },
): boolean {
  return (
    status === 401 &&
    !config.skipAuthRefresh &&
    !config._retry &&
    useSessionStore.getState().refreshToken !== null
  );
}

function getRefreshPromise(): Promise<string> {
  refreshPromise ??= performRefresh().finally(() => {
    refreshPromise = null;
  });
  return refreshPromise;
}

async function performRefresh(): Promise<string> {
  const state = useSessionStore.getState();
  const refreshToken = state.refreshToken;

  if (!refreshToken) {
    throw new EnvelopeError({ messages: [], type: ErrorType.AUTHENTICATION });
  }

  const tokens = await refreshTokens(refreshToken);

  // OpenIddict ротирует refresh-токены: старый становится недействительным,
  // поэтому сохраняем новый. Иначе следующий refresh отправит уже
  // использованный токен, сервер сочтёт это повторным использованием
  // и отзовёт всю цепочку токенов.
  useSessionStore.getState().setTokens({
    accessToken: tokens.accessToken,
    refreshToken: tokens.refreshToken ?? refreshToken,
    idToken: tokens.idToken ?? state.idToken,
    expiresAt: tokens.expiresAt,
  });

  return tokens.accessToken;
}
