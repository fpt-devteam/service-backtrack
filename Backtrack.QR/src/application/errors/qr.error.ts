import type { Error } from "@/src/shared/core/error.js";

export const QrErrors = {
  NotFound: {
    kind: "NotFound",
    code: "QrNotFound",
    message: "The requested QR code was not found.",
  } as Error
} as const satisfies Record<string, Error>;

