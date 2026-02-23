export type Error = {
  kind: "Validation" | "NotFound" | "Unauthorized" | "Conflict" | "Internal";
  code: string;
  message: string;
  cause?: unknown;
};

export function createError(kind: Error["kind"], code: string, message: string, cause?: unknown): Error {
  return { kind, code, message, cause } as Error;
}

export function isAppError(error: unknown): error is Error {
  return (
    typeof error === "object" &&
    error !== null &&
    "kind" in error &&
    "code" in error &&
    "message" in error
  );
}