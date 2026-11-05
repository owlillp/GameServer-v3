import { Spinner } from "@/src/shared/ui/spinner";

export function PageLoader({ label = "Загрузка..." }: { label?: string }) {
  return (
    <div className="flex flex-1 items-center justify-center p-10">
      <Spinner label={label} />
    </div>
  );
}
