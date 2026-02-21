import type { Error } from "@/src/shared/core/error.js";

export const ServerErrors = {
  MissingUserIdHeader: {
    kind: "Internal",
    code: "MissingUserIdHeader",
    message: "The user ID header is missing.",
  } as Error
} as const satisfies Record<string, Error>;

