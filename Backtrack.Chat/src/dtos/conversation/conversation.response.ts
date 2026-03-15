import { ConversationType } from "@/models/interfaces/conversation.interface";
export interface ConversationsListResult {
    conversations: ConversationResponse[];
    nextCursor: string | null;
    hasMore: boolean;
}
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