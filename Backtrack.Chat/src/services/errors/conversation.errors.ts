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
  PostIdMismatch: {
	kind: "Conflict",
	code: "PostIdMismatch",
	message: "A conversation for this organization already exists with a different postId.",
  } as Error,
  PostAlreadyInProgress: {
    kind: "Conflict",
    code: "PostAlreadyInProgress",
    message: "Another conversation for this post is already in progress.",
  } as Error,
  NotVerified: {
	kind: "Conflict",
	code: "ConversationNotVerified",
	message: "This conversation must be marked as verified before it can be resolved.",
  },
  NotRejectable: {
	kind: "Conflict",
	code: "ConversationNotRejectable",
	message: "Only conversations that are in progress or verified can be rejected.",
  } as Error,
} as const satisfies Record<string, Error>;
