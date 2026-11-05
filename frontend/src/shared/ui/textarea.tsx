import { cn } from "@/src/shared/lib/utils";
import { forwardRef, type TextareaHTMLAttributes } from "react";

export const Textarea = forwardRef<
  HTMLTextAreaElement,
  TextareaHTMLAttributes<HTMLTextAreaElement>
>(function Textarea({ className, ...props }, ref) {
  return (
    <textarea
      ref={ref}
      className={cn(
        "w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm",
        "placeholder:text-slate-400 focus:ring-2 focus:ring-slate-400 focus:outline-none",
        "disabled:opacity-50",
        className,
      )}
      {...props}
    />
  );
});
