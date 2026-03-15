export enum MessageType {
  TEXT = 'text',
  IMAGE = 'image',
  FILE = 'file',
  VIDEO = 'video',
}

export enum MessageStatus {
  SENT = 'sent',
  SEEN = 'seen',
  FAILED = 'failed',
}

export interface IMessageAttachment {
  type: MessageType;
  url: string;
  fileName?: string;
  fileSize?: number;
  mimeType?: string;
  thumbnail?: string; //  video/image
  duration?: number; //video (seconds)
  width?: number;    // image/video
  height?: number;   // image/video
}

export interface IMessage {
	id: string;
	conversationId: string;
	senderId: string;
	type: MessageType;
	content: string;
	attachments: IMessageAttachment[] | null;
	status: MessageStatus;
	createdAt: Date;
	updatedAt: Date;
    deletedAt: Date | null;
}