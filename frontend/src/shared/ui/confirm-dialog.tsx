"use client";

import { Button } from "@/src/shared/ui/button";
import { useEffect, useRef } from "react";

type Props = {
  open: boolean;
  title: string;
  description?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  isPending?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
};

// Нативный <dialog> (showModal): фокус-трап, Esc и инертный фон — из платформы.
export function ConfirmDialog({
  open,
  title,
  description,
  confirmLabel = "Подтвердить",
  cancelLabel = "Отмена",
  isPending = false,
  onConfirm,
  onCancel,
}: Props) {
  const dialogRef = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;

    if (open && !dialog.open) {
      dialog.showModal();
    } else if (!open && dialog.open) {
      dialog.close();
    }
  }, [open]);

  return (
    <dialog
      ref={dialogRef}
      onClose={onCancel}
      aria-labelledby="confirm-dialog-title"
      className="m-auto w-[min(26rem,calc(100vw-2rem))] rounded-lg border border-slate-200 bg-white p-6 shadow-xl backdrop:bg-slate-900/40"
    >
      <h2
        id="confirm-dialog-title"
        className="text-base font-semibold text-slate-900"
      >
        {title}
      </h2>

      {description && (
        <p className="mt-2 text-sm text-slate-600">{description}</p>
      )}

      <div className="mt-6 flex justify-end gap-3">
        <Button variant="ghost" onClick={onCancel} disabled={isPending}>
          {cancelLabel}
        </Button>
        <Button onClick={onConfirm} disabled={isPending}>
          {isPending ? "Выполняем..." : confirmLabel}
        </Button>
      </div>
    </dialog>
  );
}
