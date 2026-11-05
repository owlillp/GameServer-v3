// Зеркалит ProfileService.Contracts.Dtos.ProfileDto (camelCase).
// Объединяет игровые поля (ник, регион) и identity-поля из access-токена
// (email, userName), которые AuthService кладёт в claims.

export type PlayerProfileResponse = {
  id: string;
  nickname: string;
  region: string | null;
  email: string | null;
  userName: string | null;
  createdAt: string;
  updatedAt: string;
};
