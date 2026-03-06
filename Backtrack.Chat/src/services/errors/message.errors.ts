import type { Error } from "@/utils/api-error";

export const MessageErrors = {
  NotFound: {
    kind: "NotFound",
    code: "MessageNotFound",
    message: "The requested message was not found.",
  } as Error,
  Unauthorized: {
    kind: "Unauthorized",
    code: "MessageUnauthorized",
    message: "You are not authorized to access this message.",
  } as Error,
  ConversationNotFound: {
    kind: "NotFound",
    code: "ConversationNotFound",
    message: "The conversation does not exist.",
  } as Error,
  InvalidContent: {
    kind: "Validation",
    code: "InvalidMessageContent",
    message: "Message content is required for text messages.",
  } as Error,
} as const satisfies Record<string, Error>;