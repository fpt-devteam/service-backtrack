import type { Error } from "@/src/shared/errors/error.js";
export const OrderErrors = {
    NotFound: {
        kind: "NotFound",
        code: "OrderNotFound",
        message: "The requested order was not found.",
    }
} as const satisfies Record<string, Error>;
