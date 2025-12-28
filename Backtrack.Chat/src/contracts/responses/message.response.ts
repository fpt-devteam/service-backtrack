import { MessageType, MessageStatus } from '@src/models/message.model';

export interface MessageResponse {
  id: string;
  conversationId: string;
  senderId: string;
  type: MessageType;
  content: string;
  status?: MessageStatus;
  createdAt: string;
  updatedAt: string;
}
