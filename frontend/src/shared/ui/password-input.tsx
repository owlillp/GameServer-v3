"use client";

import { Input } from "@/src/shared/ui/input";
import { cn } from "@/src/shared/lib/utils";
import { forwardRef, useState, type InputHTMLAttributes } from "react";

// Поле пароля с кнопкой «Показать/Скрыть». Позволяет пользователю проверить
// ввод, не мешая менеджерам паролей (type остаётся password до клика).
export const PasswordInput = forwardRef<
  HTMLInputElement,
  InputHTMLAttributes<HTMLInputElement>
>(function PasswordInput({ className, ...props }, ref) {
  const [visible, setVisible] = useState(false);

  return (
    <div className="relative">
      <Input
        ref={ref}
        {...props}
        type={visible ? "text" : "password"}
        className={cn("pr-20", className)}
      />
      <button
        type="button"
        onClick={() => setVisible((value) => !value)}
        aria-pressed={visible}
        aria-label={visible ? "Скрыть пароль" : "Показать пароль"}
        className="absolute inset-y-0 right-0 px-3 text-xs font-medium text-slate-500 hover:text-slate-900"
      >
        {visible ? "Скрыть" : "Показать"}
      </button>
    </div>
  );
});
