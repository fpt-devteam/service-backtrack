import type { Error } from "@/src/shared/errors/error.js";
import { ForbiddenError } from "@payos/node";

export const QrCodeErrors = {
  NotFound: {
    kind: "NotFound",
    code: "QrCodeNotFound",
    message: "The requested QR code was not found.",
  } as Error,
  RequireAtLeastOneField: {
    kind: "Validation",
    code: "RequireAtLeastOneField",
    message: "At least one field must be provided for update.",
  } as Error,
   Forbidden: {
    kind: "Validation",
    code: "ForbiddenQrCodeAccess",
    message: "You do not have permission to perform this action on the QR code.",
  } as Error,
} as const satisfies Record<string, Error>;

