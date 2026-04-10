import type { Error } from "@/utils/api-error";

export const ConversationErrors = {
  NotFound: {
    kind: "NotFound",
    code: "ConversationNotFound",
    message: "The requested conversation was not found.",
  } as Error,
  AlreadyExists: {
    kind: "Conflict",
    code: "ConversationAlreadyExists",
    message: "A conversation between these users already exists.",
  } as Error,
  Unauthorized: {
    kind: "Unauthorized",
    code: "ConversationUnauthorized",
    message: "You are not authorized to access this conversation.",
  } as Error,
  InvalidParticipants: {
    kind: "Validation",
    code: "InvalidParticipants",
    message: "Invalid participants for this conversation type.",
  } as Error,
  InvalidConversationType: {
    kind: "Validation",
    code: "InvalidConversationType",
    message: "This operation is only available for organization conversations.",
  } as Error,
  NotInQueue: {
    kind: "Conflict",
    code: "ConversationNotInQueue",
    message: "This conversation is not in the queue or has already been taken.",
  } as Error,
  NotAssigned: {
    kind: "Conflict",
    code: "ConversationNotAssigned",
    message: "This staff member is not currently assigned to this conversation.",
  } as Error,
  OrgNotFound: {
    kind: "NotFound",
    code: "OrgNotFound",
    message: "The organization was not found. It may not have been synced yet.",
  } as Error,
} as const satisfies Record<string, Error>;
