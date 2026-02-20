import type { Error } from "@/src/shared/core/error.js";

export const UserErrors = {
  NotFound: {
    kind: "NotFound",
    code: "UserNotFound",
    message: "The requested user was not found.",
  } as Error,
  EmailExists: {
    kind: "Conflict",
    code: "EmailAlreadyExists",
    message: "A user with the given email already exists.",
  } as Error,
} as const satisfies Record<string, Error>;

