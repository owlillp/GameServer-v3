import axios from "axios";

export const ErrorType = {
  VALIDATION: "VALIDATION",
  NOT_FOUND: "NOT_FOUND",
  FAILURE: "FAILURE",
  CONFLICT: "CONFLICT",
  AUTHENTICATION: "AUTHENTICATION",
  AUTHORIZATION: "AUTHORIZATION",
} as const;

export type ErrorType = (typeof ErrorType)[keyof typeof ErrorType];

export type ErrorMessage = {
  code: string;
  message: string;
  invalidField?: string | null;
};

export type ApiError = {
  messages: ErrorMessage[];
  type: ErrorType;
  isCritical?: boolean;
};

// Строка ошибки для UI: код + текст (+ поле, если ошибка валидации поля).
export type ErrorLine = {
  code: string;
  message: string;
  invalidField?: string | null;
};

export class EnvelopeError extends Error {
  public readonly apiError: ApiError;
  public readonly type: ErrorType;

  constructor(apiError: ApiError) {
    super(apiError.messages[0]?.message ?? "Неизвестная ошибка");

    this.name = "EnvelopeError";
    this.apiError = apiError;
    this.type = apiError.type;

    Object.setPrototypeOf(this, EnvelopeError.prototype);
  }

  get messages(): ErrorMessage[] {
    return this.apiError.messages;
  }

  get firstError(): ErrorMessage | undefined {
    return this.apiError.messages[0];
  }

  get allMessages(): string[] {
    return this.apiError.messages.map((error) => error.message);
  }
}

export function isEnvelopeError(error: unknown): error is EnvelopeError {
  return error instanceof EnvelopeError;
}

export function getErrorMessage(error: unknown, fallback: string): string {
  if (isEnvelopeError(error)) {
    return error.firstError?.message || fallback;
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return fallback;
}

// Приводит любую ошибку к списку строк с кодом, чтобы UI показывал не только
// сообщение, но и код (Envelope-код, HTTP-статус, имя/код ошибки).
export function getErrorLines(error: unknown, fallback: string): ErrorLine[] {
  if (isEnvelopeError(error)) {
    if (error.messages.length > 0) return error.messages;
    return [{ code: "UNKNOWN", message: fallback }];
  }

  if (axios.isAxiosError(error)) {
    const status = error.response?.status;
    const code = status ? `HTTP ${status}` : (error.code ?? "NETWORK");
    return [{ code, message: getErrorMessage(error, fallback) }];
  }

  if (error instanceof Error && error.message) {
    return [{ code: error.name || "ERROR", message: error.message }];
  }

  return [{ code: "UNKNOWN", message: fallback }];
}

export function isForbiddenError(error: unknown): boolean {
  if (isEnvelopeError(error)) {
    return error.type === ErrorType.AUTHORIZATION;
  }

  return axios.isAxiosError(error) && error.response?.status === 403;
}
