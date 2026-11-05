"use client";

import { queryClient } from "@/src/shared/api/query-client";
import {
  ErrorBoundary,
  DefaultFallback,
} from "@/src/shared/components/errorBoundary/error-boundary";
import { QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "sonner";

export default function Providers({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <QueryClientProvider client={queryClient}>
      <ErrorBoundary
        FallbackComponent={DefaultFallback}
        onError={(error, info) => {
          console.error("[ErrorBoundary] Caught an error:", error, info);
        }}
      >
        <div className="flex min-h-svh w-full flex-col">{children}</div>
      </ErrorBoundary>
      <Toaster position="top-center" duration={3000} richColors />
    </QueryClientProvider>
  );
}
