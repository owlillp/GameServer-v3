import { ProfileCard } from "@/src/features/profile";
import { RequireAuth } from "@/src/shared/auth/require-auth";
import { SiteHeader } from "@/src/shared/ui/site-header";

export default function ProfilePage() {
  return (
    <RequireAuth>
      <SiteHeader />

      <main className="mx-auto w-full max-w-2xl flex-1 space-y-6 px-6 py-10">
        <header className="space-y-1">
          <h1 className="text-2xl font-semibold text-slate-900">Мой профиль</h1>
          <p className="text-xs text-slate-500">
            GET /profiles/me — защищён OIDC access-токеном (Bearer). ProfileService
            отдаёт игровой профиль и identity-поля из claims.
          </p>
        </header>

        <section className="rounded-lg border border-slate-200 bg-white p-6">
          <ProfileCard />
        </section>

      </main>
    </RequireAuth>
  );
}
