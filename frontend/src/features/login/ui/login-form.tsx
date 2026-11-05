"use client";

import { ApiErrorList } from "@/src/shared/ui/api-error-list";
import { Button } from "@/src/shared/ui/button";
import { Input } from "@/src/shared/ui/input";
import { PasswordInput } from "@/src/shared/ui/password-input";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { loginSchema, type LoginFormValues } from "../model/schema";
import { useLogin } from "../model/use-login";

export function LoginForm({
  returnTo = "",
  returnUrl,
}: {
  returnTo?: string;
  returnUrl?: string | null;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    mode: "onTouched",
    defaultValues: { email: "", password: "" },
  });

  const loginMutation = useLogin({ returnTo, returnUrl });

  const onSubmit = handleSubmit(async (values) => {
    try {
      // Успех уводит браузер на /connect/authorize — дальше загрузка не нужна.
      await loginMutation.login(values);
    } catch {
      // Ошибка уже в loginMutation.error — отрисуем ниже.
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
          autoComplete="username"
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
          htmlFor="current-password"
        >
          Пароль
        </label>
        <PasswordInput
          id="current-password"
          autoComplete="current-password"
          required
          enterKeyHint="done"
          {...register("password")}
        />
        {errors.password && (
          <p className="mt-1 text-xs text-red-600">{errors.password.message}</p>
        )}
      </div>

      <Button
        type="submit"
        disabled={isSubmitting || loginMutation.isPending}
        className="w-full"
      >
        {loginMutation.isPending ? "Входим..." : "Войти"}
      </Button>

      <ApiErrorList error={loginMutation.error} fallback="Не удалось войти" />
    </form>
  );
}
