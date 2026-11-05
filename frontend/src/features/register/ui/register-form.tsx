"use client";

import { routes } from "@/src/shared/routes";
import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { Input } from "@/src/shared/ui/input";
import { PasswordInput } from "@/src/shared/ui/password-input";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { registerSchema, type RegisterFormValues } from "../model/schema";
import { useRegister } from "../model/use-register";

export function RegisterForm({
  returnUrl = null,
}: {
  returnUrl?: string | null;
}) {
  const router = useRouter();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    mode: "onTouched",
    defaultValues: { email: "", userName: "", password: "" },
  });

  const registerMutation = useRegister();

  const onSubmit = handleSubmit(async (values) => {
    try {
      await registerMutation.register(values);
      // После регистрации токен не выдаётся — отправляем на вход,
      // сохраняя внешний returnUrl (например, из Unity-флоу).
      const loginHref = returnUrl
        ? `${routes.login}?returnUrl=${encodeURIComponent(returnUrl)}`
        : routes.login;
      router.push(loginHref);
    } catch {
      // Ошибка уже в registerMutation.error — отрисуем ниже.
    }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4" noValidate>
      <div>
        <label
          className="mb-1 block text-sm font-medium text-slate-700"
          htmlFor="email"
        >
          Email
        </label>
        <Input
          id="email"
          type="email"
          autoComplete="email"
          required
          enterKeyHint="next"
          {...register("email")}
        />
        {errors.email && (
          <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>
        )}
      </div>

      <div>
        <label
          className="mb-1 block text-sm font-medium text-slate-700"
          htmlFor="userName"
        >
          Имя пользователя
        </label>
        <Input
          id="userName"
          autoComplete="username"
          required
          enterKeyHint="next"
          {...register("userName")}
        />
        {errors.userName && (
          <p className="mt-1 text-xs text-red-600">{errors.userName.message}</p>
        )}
      </div>

      <div>
        <label
          className="mb-1 block text-sm font-medium text-slate-700"
          htmlFor="new-password"
        >
          Пароль
        </label>
        <PasswordInput
          id="new-password"
          autoComplete="new-password"
          required
          enterKeyHint="done"
          minLength={8}
          aria-describedby="password-constraints"
          {...register("password")}
        />
        <p id="password-constraints" className="mt-1 text-xs text-slate-500">
          Минимум 8 символов, строчная и заглавная буквы, цифра.
        </p>
        {errors.password && (
          <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>
        )}
      </div>

      <Button
        type="submit"
        disabled={isSubmitting || registerMutation.isPending}
        className="w-full"
      >
        {registerMutation.isPending ? "Регистрируем..." : "Зарегистрироваться"}
      </Button>

      <ApiErrorList
        error={registerMutation.error}
        fallback="Не удалось зарегистрироваться"
      />
    </form>
  );
}
