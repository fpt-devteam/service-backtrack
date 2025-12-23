import type { Error } from "@/src/errors/error.js";

export const QrCodeErrors = {
    NotFound: {
        kind: "NotFound",
        code: "QrCodeNotFound",
        message: "The requested QR code was not found.",
    } as Error
} as const satisfies Record<string, Error>;

