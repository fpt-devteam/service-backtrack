import type { Error } from "@/src/shared/errors/error.js";
export const TransactionError = {
    TransactionNotFound: {
        kind: "NotFound",
        code: "TransactionNotFound",
        message: "The requested transaction was not found.",
    },
    
} as const satisfies Record<string, Error>;