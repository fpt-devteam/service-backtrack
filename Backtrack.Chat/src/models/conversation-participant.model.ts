import { Schema, model, Document } from 'mongoose';
export interface IConversationParticipant extends Document {
  conversationId: Schema.Types.ObjectId | string;
  memberId: string;
  
  partnerDisplayName?: string | null;

  lastReadAt?: Date | null;       
  lastReadMessageId?: Schema.Types.ObjectId | string | null; 
  unreadCount?: number;

  deletedAt?: Date | null;
  createdAt: Date;
  updatedAt: Date;
}

const ConversationParticipantSchema = new Schema<IConversationParticipant>(
  {
    conversationId: {
      type: Schema.Types.ObjectId,
      ref: 'Conversation',
      required: true,
    },
    memberId: { type: String, required: true, ref: 'User' },

    partnerDisplayName: { type: String, default: null },
    

    lastReadAt: { type: Date, default: null },
    lastReadMessageId: { 
      type: Schema.Types.ObjectId, 
      ref: 'Message',
      default: null,
    },
    unreadCount: { type: Number, default: 0, min: 0 },
    deletedAt: { type: Date, default: null },
  },
  { timestamps: true },
);

ConversationParticipantSchema.index({
  conversationId: 1,
  memberId: 1,
  deletedAt: 1,
});

ConversationParticipantSchema.index({ memberId: 1, deletedAt: 1 });
ConversationParticipantSchema.index({ conversationId: 1, deletedAt: 1 });

ConversationParticipantSchema.index(
  { conversationId: 1, memberId: 1 },
  { unique: true, partialFilterExpression: { deletedAt: null } },
);

const ConversationParticipant = model<IConversationParticipant>(
  'ConversationParticipant',
  ConversationParticipantSchema,
);

export default ConversationParticipant;
