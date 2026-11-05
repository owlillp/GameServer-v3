import { cn } from "@/src/shared/lib/utils";
import type { ButtonHTMLAttributes } from "react";

type Props = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: "primary" | "ghost";
};

const base =
  "inline-flex h-9 items-center justify-center rounded-md text-sm font-medium transition-colors disabled:pointer-events-none disabled:opacity-50";

const variants = {
  primary: "bg-slate-900 px-4 text-white hover:bg-slate-800",
  ghost: "bg-transparent px-3 text-slate-700 hover:bg-slate-100",
};

export function Button({ className, variant = "primary", ...props }: Props) {
  return (
    <button className={cn(base, variants[variant], className)} {...props} />
  );
}
