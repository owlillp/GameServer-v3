import { apiClient } from "@/src/shared/api/axios-instance";
import { Envelope, unwrapEnvelope } from "@/src/shared/api/envelope";
import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
} from "./types";

export const authApi = {
  register: async (request: RegisterRequest): Promise<RegisterResponse> => {
    const response = await apiClient.post<Envelope<RegisterResponse>>(
      "/auth/register",
      request,
      { skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },

  // Ставит Identity-cookie — browser session для последующего /connect/authorize.
  login: async (request: LoginRequest): Promise<LoginResponse> => {
    const response = await apiClient.post<Envelope<LoginResponse>>(
      "/auth/login",
      request,
      { skipAuthRefresh: true },
    );

    return unwrapEnvelope(response.data);
  },
};

export const authQueryKeys = {
  all: ["auth"] as const,
};
