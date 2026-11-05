import { RegisterForm } from "@/src/features/register";
import { RedirectIfAuthenticated } from "@/src/shared/auth/redirect-if-authenticated";
import { sanitizeAuthReturnUrl } from "@/src/shared/auth/return-url";
import { routes } from "@/src/shared/routes";
import { Gamepad2 } from "lucide-react";
import Link from "next/link";

type SearchParams = {
  returnUrl?: string | string[];
};

export default async function RegisterPage({
  searchParams,
}: {
  searchParams: Promise<SearchParams>;
}) {
  const params = await searchParams;
  const rawReturnUrl = Array.isArray(params.returnUrl)
    ? params.returnUrl[0]
    : params.returnUrl;
  const returnUrl = sanitizeAuthReturnUrl(rawReturnUrl);

  const loginHref = returnUrl
    ? `${routes.login}?returnUrl=${encodeURIComponent(returnUrl)}`
    : routes.login;

  return (
    <main className="mx-auto flex w-full max-w-md flex-1 flex-col justify-center px-6 py-10">
      <Link
        href={routes.home}
        className="mb-6 inline-flex items-center gap-2 text-sm font-semibold tracking-tight text-slate-900"
      >
        <span
          aria-hidden="true"
          className="inline-flex h-7 w-7 items-center justify-center rounded-lg bg-slate-900 text-white"
        >
          <Gamepad2 className="h-4 w-4" />
        </span>
        GameServer v3
      </Link>

      <h1 className="mb-6 text-2xl font-semibold text-slate-900">
        Регистрация
      </h1>

      <RedirectIfAuthenticated>
        <section className="rounded-lg border border-slate-200 bg-white p-6">
          <RegisterForm returnUrl={returnUrl} />
        </section>

        <p className="mt-4 text-sm text-slate-600">
          Уже есть аккаунт?{" "}
          <Link
            href={loginHref}
            className="font-medium text-slate-900 underline"
          >
            Войти
          </Link>
        </p>
      </RedirectIfAuthenticated>
    </main>
  );
}
