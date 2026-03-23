import { ConversationStatus } from '@/models';

// ─── Shared sub-types ────────────────────────────────────────────────────────

export interface ConversationPartner {
  id: string;
  displayName: string | null;
  email: string | null;
  avatarUrl: string | null;
}

export interface ConversationLastMessage {
  senderId: string | null;
  content: string;
  timestamp: Date | null;
}

// ─── Direct Conversation ──────────────────────────────────────────────────────

/**
 * Response DTO for a Direct (1-on-1) conversation.
 * Returned by: GET /conversations/partner/:partnerId, POST /conversations/direct
 */
export interface DirectConversationResponse {
  /** MongoDB _id as string */
  conversationId: string;
  /** The other participant in this DM */
  partner: ConversationPartner | null;
  lastMessage: ConversationLastMessage | null;
  unreadCount: number;
  createdAt: Date;
  updatedAt: Date;
}

// ─── Support Conversation ─────────────────────────────────────────────────────

/**
 * Response DTO for a Support (org-based) conversation.
 * Returned by: POST /conversations/org, GET /conversations/:id (when orgId present)
 */
export interface SupportConversationResponse {
  /** MongoDB _id as string */
  conversationId: string;
  /** Organisation this conversation belongs to */
  orgId: string;
  /** Current lifecycle status */
  status: ConversationStatus;
  /** Staff member currently handling this conversation, null if in queue */
  assignedStaffId: string | null;
  /** The customer who opened the conversation */
  partner: ConversationPartner | null;
  lastMessage: ConversationLastMessage | null;
  unreadCount: number;
  createdAt: Date;
  updatedAt: Date;
}

// ─── Generic / list  ─────────────────────────────────────────────────────────

/**
 * Unified response shape used in GET /conversations (mixed list).
 * type discriminates between Direct and Support rows.
 */
export interface ConversationResponse {
  conversationId: string;
  /** 'Direct' = DM, 'organization' = support channel */
  type: string;
  orgId: string | null;
  status: ConversationStatus | null;
  assignedStaffId: string | null;
  partner: ConversationPartner | null;
  lastMessage: ConversationLastMessage | null;
  unreadCount: number;
  createdAt: Date;
  updatedAt: Date;
}

/** Generic mixed-list (kept for future use if needed) */
export interface ConversationsListResult {
  conversations: ConversationResponse[];
  nextCursor: string | null;
  hasMore: boolean;
}

/** Typed list for Direct (DM) conversations only */
export interface DirectConversationsListResult {
  conversations: DirectConversationResponse[];
  nextCursor: string | null;
  hasMore: boolean;
}

/** Typed list for Support (org) conversations only */
export interface SupportConversationsListResult {
  conversations: SupportConversationResponse[];
  nextCursor: string | null;
  hasMore: boolean;
}