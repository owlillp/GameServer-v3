import { cn } from "@/src/shared/lib/utils";
import { Role } from "@/src/shared/auth/roles";

const ROLE_STYLES: Record<string, string> = {
  [Role.ADMIN]: "bg-rose-50 text-rose-700 ring-rose-200",
  [Role.MODERATOR]: "bg-amber-50 text-amber-700 ring-amber-200",
  [Role.USER]: "bg-slate-100 text-slate-700 ring-slate-200",
  [Role.SERVICE]: "bg-sky-50 text-sky-700 ring-sky-200",
};

export function RoleBadge({
  role,
  className,
}: {
  role: string;
  className?: string;
}) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset",
        ROLE_STYLES[role] ?? "bg-slate-100 text-slate-600 ring-slate-200",
        className,
      )}
    >
      {role}
    </span>
  );
}
