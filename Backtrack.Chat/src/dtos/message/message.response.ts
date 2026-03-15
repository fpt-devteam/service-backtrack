import { IMessageAttachment, MessageStatus, MessageType } from "@/models";


export interface MessageResponse {
  id: string;
  conversationId: string;
  senderId: string;
  type: MessageType;
  content: string;
  attachments?: IMessageAttachment[];
  status: MessageStatus;
  createdAt: Date;
  updatedAt: Date;
}

export interface MessagesResponse {
  messages: MessageResponse[];
  nextCursor: string | null;
  hasMore: boolean;
}
