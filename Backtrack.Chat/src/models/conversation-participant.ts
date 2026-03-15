import { Schema, model } from 'mongoose';
import { ConversationParticipantRole, IConversationParticipant } from './interfaces/conversation-participant.interface';


const ConversationParticipantSchema = new Schema<IConversationParticipant>(
    {
        conversationId: { type: String, required: true, index: true },
        memberId: { type: String, required: false, index: true },
        role: { type: String, enum: Object.values(ConversationParticipantRole), required: true },
        isAssigee: { type: Boolean, default: false },
        nickName: { type: String, default: null },

        lastReadAt: { type: Date, default: null },
        lastReadMessageId: { type: String, default: null },
        unreadCount: { type: Number, default: 0 },

        deletedAt: { type: Date, default: null },
     },
     {
         timestamps: true,
         minimize: false,
      },
 );
 
ConversationParticipantSchema.index({ conversationId: 1, memberId: 1 }, { unique: true });
const ConversationParticipant = model<IConversationParticipant>('ConversationParticipant', ConversationParticipantSchema);
export default ConversationParticipant;

