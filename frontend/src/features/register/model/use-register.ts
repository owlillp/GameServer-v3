import { authApi, authQueryKeys } from "@/src/entities/auth";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

export function useRegister() {
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: authApi.register,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: authQueryKeys.all });
      toast.success("Аккаунт создан. Войдите в систему.");
    },
  });

  return {
    register: mutation.mutateAsync,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
  };
}
