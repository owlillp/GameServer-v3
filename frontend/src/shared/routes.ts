export const routes = {
  home: "/",
  login: "/login",
  register: "/register",
  profile: "/profile",
} as const;

export type RouteKey = keyof typeof routes;
