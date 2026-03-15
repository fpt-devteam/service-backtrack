import { Schema, model } from 'mongoose';
import { IConversationQueue } from './interfaces/conversation-queue.interface';

const ConversationQueueSchema = new Schema<IConversationQueue>(
    {
        conversationId: { type: String, required: true, unique: true, index: true },
        deletedAt: { type: Date, default: null },
        takenAt: { type: Date, default: null },
        takenBy: { type: String, default: null },
    },
    {
        timestamps: true,
        minimize: false,
    },
);

ConversationQueueSchema.index({ takenAt: 1, deletedAt: 1 });
ConversationQueueSchema.index({ takenBy: 1, takenAt: 1 });

const ConversationQueue = model<IConversationQueue>(
    'ConversationQueue',
    ConversationQueueSchema,
);
export default ConversationQueue;
