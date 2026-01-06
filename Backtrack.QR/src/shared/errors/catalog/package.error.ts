import type { Error } from "@/src/shared/errors/error.js";
export const PackageErrors = {
    NotFound: {
        kind: "NotFound",
        code: "PackageNotFound",
        message: "The requested package was not found.",
    }
} as const satisfies Record<string, Error>;