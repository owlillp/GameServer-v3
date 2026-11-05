import { cn } from "@/src/shared/lib/utils";
import { forwardRef, type InputHTMLAttributes } from "react";

export const Input = forwardRef<
  HTMLInputElement,
  InputHTMLAttributes<HTMLInputElement>
>(function Input({ className, ...props }, ref) {
  return (
    <input
      ref={ref}
      className={cn(
        "h-9 w-full rounded-md border border-slate-300 bg-white px-3 text-sm",
        "placeholder:text-slate-400 focus:ring-2 focus:ring-slate-400 focus:outline-none",
        "disabled:opacity-50",
        className,
      )}
      {...props}
    />
  );
});
