import type { Error } from "@/src/shared/core/error.js";

export const ServerErrors = {
  MissingUserIdHeader: {
    kind: "Internal",
    code: "MissingUserIdHeader",
    message: "The user ID header is missing.",
  } as Error,
  ProviderCustomerIdNotFound: {
    kind: "Internal",
    code: "ProviderCustomerIdNotFound",
    message: "No user found with the given provider customer ID.",
  } as Error,
} as const satisfies Record<string, Error>;

