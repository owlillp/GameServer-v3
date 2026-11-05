import { getErrorLines } from "@/src/shared/api/errors";
import { cn } from "@/src/shared/lib/utils";

type Props = {
  error: unknown;
  fallback: string;
  className?: string;
};

// Показывает ошибку вместе с кодом: Envelope-код, HTTP-статус или имя ошибки.
// role="alert" — для скринридеров.
export function ApiErrorList({ error, fallback, className }: Props) {
  if (!error) return null;

  const lines = getErrorLines(error, fallback);

  return (
    <div
      role="alert"
      className={cn("space-y-1 text-sm text-red-600", className)}
    >
      {lines.map((line, index) => (
        <p
          key={`${line.code}-${index}`}
          className="flex flex-wrap items-baseline gap-x-2"
        >
          <code className="rounded bg-red-50 px-1.5 py-0.5 font-mono text-xs text-red-700">
            {line.code}
          </code>
          <span>{line.message}</span>
        </p>
      ))}
    </div>
  );
}
