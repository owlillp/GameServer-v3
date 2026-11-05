import { cn } from "@/src/shared/lib/utils";
import { forwardRef, type SelectHTMLAttributes } from "react";

export const Select = forwardRef<
  HTMLSelectElement,
  SelectHTMLAttributes<HTMLSelectElement>
>(function Select({ className, ...props }, ref) {
  return (
    <select
      ref={ref}
      className={cn(
        "h-9 w-full rounded-md border border-slate-300 bg-white px-3 text-sm",
        "focus:ring-2 focus:ring-slate-400 focus:outline-none disabled:opacity-50",
        className,
      )}
      {...props}
    />
  );
});
