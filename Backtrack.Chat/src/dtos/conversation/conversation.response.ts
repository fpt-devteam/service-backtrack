import { ConversationType } from "@/models/conversation";
export interface ConversationsListResult {
    conversations: ConversationResponse[];
    nextCursor: string | null;
    hasMore: boolean;
}
export interface ConversationPartner {
  id: string;
  displayName?: string | null;
  email: string | null;
  avatarUrl?: string | null;
}

export interface ConversationLastMessage {
  senderId?: string;
  content?: string;
  timestamp?: Date;
}

export interface ConversationResponse {
  conversationId: string;
  type: ConversationType;
  partner: ConversationPartner | null;
  orgId: string | null;
  lastMessage: ConversationLastMessage | null;
  unreadCount: number;
  createdAt: Date;
  updatedAt: Date;
}