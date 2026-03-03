import { Schema, model } from 'mongoose';
export enum ConversationParticipantRole {
    CUSTOMER = 'customer',
    STAFF = 'staff',
}
export interface IConversationParticipant {
	conversationId: Schema.Types.ObjectId | string;
	memberId: string;
    orgId?: string | null;
    role: ConversationParticipantRole;
    isAssigee?: boolean;
	nickName?: string | null;

	lastReadAt?: Date | null;
	lastReadMessageId?: Schema.Types.ObjectId | string | null;
	unreadCount?: number;

	deletedAt?: Date | null;
	createdAt: Date;
	updatedAt: Date;
}

const ConversationParticipantSchema = new Schema<IConversationParticipant>(
    {
        conversationId: { type: Schema.Types.ObjectId, required: true, index: true },
        memberId: { type: String, required: true, index: true },
        orgId: { type: String, default: null, index: true },
        role: { type: String, enum: Object.values(ConversationParticipantRole), required: true },
        isAssigee: { type: Boolean, default: false },
        nickName: { type: String, default: null },

        lastReadAt: { type: Date, default: null },
        lastReadMessageId: { type: Schema.Types.ObjectId, default: null },
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

