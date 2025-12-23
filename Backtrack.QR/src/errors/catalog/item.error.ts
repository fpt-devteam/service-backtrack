import type { Error } from "@/src/errors/error.js";

export const ItemErrors = {
    NotFound: {
        kind: "NotFound",
        code: "ItemNotFound",
        message: "The requested item was not found.",
    } as Error,
    InvalidName: {
        kind: "Validation",
        code: "InvalidItemName",
        message: "The item name provided is invalid.",
    } as Error,
} as const satisfies Record<string, Error>;

