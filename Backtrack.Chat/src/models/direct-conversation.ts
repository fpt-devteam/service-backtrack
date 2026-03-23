import { Schema, model } from 'mongoose';
import { IDirectConversation } from './interfaces/direct-conversation.interface';

const DirectConversationSchema = new Schema<IDirectConversation>(
    {
        lastMessageAt: { type: Date },
        lastMessageContent: { type: String, default: null },
        senderId: { type: String, default: null },
        deletedAt: { type: Date, default: null },
    },
    {
        timestamps: true,
        minimize: false,
    },
);

DirectConversationSchema.index({ lastMessageAt: -1 });
DirectConversationSchema.index({ senderId: 1, lastMessageAt: -1 });
DirectConversationSchema.index({ deletedAt: 1 });

const DirectConversation = model<IDirectConversation>('DirectConversation', DirectConversationSchema);
export default DirectConversation;