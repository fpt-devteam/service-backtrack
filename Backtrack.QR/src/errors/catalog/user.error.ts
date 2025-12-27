import type { Error } from "@/src/errors/error.js";

export const UserErrors = {
    NotFound: {
        kind: "NotFound",
        code: "UserNotFound",
        message: "The requested user was not found.",
    } as Error,
} as const satisfies Record<string, Error>;

