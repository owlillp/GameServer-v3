import { cn } from "@/src/shared/lib/utils";

type Props = {
  className?: string;
  label?: string;
};

export function Spinner({ className, label }: Props) {
  return (
    <span className="inline-flex items-center gap-2 text-sm text-slate-500">
      <span
        aria-hidden="true"
        className={cn(
          "h-4 w-4 animate-spin rounded-full border-2 border-slate-300 border-t-slate-900",
          className,
        )}
      />
      {label && <span>{label}</span>}
    </span>
  );
}
