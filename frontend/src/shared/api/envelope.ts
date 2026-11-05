import { ApiError, EnvelopeError, ErrorType } from "./errors";

// Зеркалит контракт Shared.SharedKernel.Envelope на стороне .NET:
// { result, error, isError, timeGenerated }.
export type Envelope<T = unknown> = {
  result: T | null;
  error: ApiError | null;
  isError: boolean;
  timeGenerated: string;
};

export function unwrapEnvelope<T>(envelope: Envelope<T>): T {
  if (
    envelope.isError ||
    envelope.result === null ||
    envelope.result === undefined
  ) {
    throw new EnvelopeError(
      envelope.error ?? { messages: [], type: ErrorType.FAILURE },
    );
  }

  return envelope.result;
}

// Для команд без результата (delete/update без тела): проверяем только ошибку.
export function assertEnvelopeSuccess(envelope: Envelope): void {
  if (envelope.isError) {
    throw new EnvelopeError(
      envelope.error ?? { messages: [], type: ErrorType.FAILURE },
    );
  }
}
