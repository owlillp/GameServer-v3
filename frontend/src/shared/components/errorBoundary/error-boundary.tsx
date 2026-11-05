"use client";

import { AlertCircleIcon, RefreshCwIcon } from "lucide-react";
import { ErrorBoundary } from "react-error-boundary";
import type { FallbackProps } from "react-error-boundary";

export { ErrorBoundary };

export function DefaultFallback({ error, resetErrorBoundary }: FallbackProps) {
  return (
    <div className="flex flex-1 flex-col items-center justify-center p-8">
      <div className="w-full max-w-md rounded-xl border p-6">
        <div className="flex items-center gap-3">
          <div className="flex size-10 items-center justify-center rounded-full">
            <AlertCircleIcon className="size-5" />
          </div>
          <h2 className="text-lg font-semibold">Что-то пошло не так</h2>
        </div>

        <p className="mt-4 text-sm">
          {error instanceof Error
            ? error.message
            : "Произошла непредвиденная ошибка"}
        </p>

        {error instanceof Error && (
          <p className="mt-2 flex flex-wrap items-baseline gap-x-2 text-xs text-slate-500">
            <code className="rounded bg-slate-100 px-1.5 py-0.5 font-mono">
              {error.name || "Error"}
            </code>
            {typeof (error as { digest?: string }).digest === "string" && (
              <code className="rounded bg-slate-100 px-1.5 py-0.5 font-mono">
                digest: {(error as { digest?: string }).digest}
              </code>
            )}
          </p>
        )}

        <button
          type="button"
          onClick={resetErrorBoundary}
          className="mt-4 inline-flex items-center gap-2 rounded-md border px-3 py-2 text-sm font-medium transition-colors"
        >
          <RefreshCwIcon className="size-4" />
          Попробовать снова
        </button>
      </div>
    </div>
  );
}
