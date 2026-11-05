import { z } from "zod";

// Ограничения зеркалят AuthService.Domain.AccountConstants и IdentitySettings:
// email <= 256, userName <= 30, пароль >= 8 (строчная/заглавная/цифра).
export const registerSchema = z.object({
  email: z
    .string()
    .min(1, "Email обязателен")
    .email("Некорректный email")
    .max(256, "Email слишком длинный"),
  userName: z
    .string()
    .min(1, "Имя пользователя обязательно")
    .max(30, "Не более 30 символов"),
  password: z
    .string()
    .min(8, "Пароль минимум 8 символов")
    .regex(/[a-z]/, "Нужна хотя бы одна строчная буква")
    .regex(/[A-Z]/, "Нужна хотя бы одна заглавная буква")
    .regex(/[0-9]/, "Нужна хотя бы одна цифра"),
});

export type RegisterFormValues = z.infer<typeof registerSchema>;
