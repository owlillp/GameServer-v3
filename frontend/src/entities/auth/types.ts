// Зеркалит контракты AuthService.Contracts (.NET PropertyNamingPolicy = camelCase).

export type RegisterRequest = {
  email: string;
  userName: string;
  password: string;
};

export type RegisterResponse = {
  accountId: string;
};

export type LoginRequest = {
  email: string;
  password: string;
};

// POST /auth/login — ставит Identity-cookie (нужна для шага /connect/authorize).
export type LoginResponse = {
  accountId: string;
  email: string;
  userName: string;
};
