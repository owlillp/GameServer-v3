import { apiClient } from "@/src/shared/api/axios-instance";
import { Envelope, unwrapEnvelope } from "@/src/shared/api/envelope";
import { queryOptions } from "@tanstack/react-query";
import type { PlayerProfileResponse } from "./types";

export const playerProfileApi = {
  // GET /profiles/me — игровой профиль текущего игрока (ProfileService).
  me: async (signal?: AbortSignal): Promise<PlayerProfileResponse> => {
    const response = await apiClient.get<Envelope<PlayerProfileResponse>>(
      "/profiles/me",
      { signal },
    );

    return unwrapEnvelope(response.data);
  },
};

export const playerProfileQueryKeys = {
  all: ["player-profile"] as const,
  me: () => [...playerProfileQueryKeys.all, "me"] as const,
};

export const playerProfileQueryOptions = () =>
  queryOptions({
    queryKey: playerProfileQueryKeys.me(),
    queryFn: ({ signal }) => playerProfileApi.me(signal),
    retry: false,
    staleTime: 0,
  });
