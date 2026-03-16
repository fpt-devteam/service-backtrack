import { Schema, model } from 'mongoose';
import { IConversationAssignment } from './interfaces/conversation-assignment.interface';

const ConversationAssignmentSchema = new Schema<IConversationAssignment>(
    {
        conversationId: { type: String, required: true, index: true },
        agentId: { type: String, required: true, index: true },
        assignedAt: { type: Date, default: null },
        assignedBy: { type: String, default: null },
        unassignedAt: { type: Date, default: null },
        note: { type: String, default: null },
        deletedAt: { type: Date, default: null },
    },
    {
        timestamps: true,
        minimize: false,
    },
);

ConversationAssignmentSchema.index({ conversationId: 1, assignedAt: -1 });
ConversationAssignmentSchema.index({ conversationId: 1, unassignedAt: 1 });
ConversationAssignmentSchema.index({ agentId: 1, unassignedAt: 1 });

const ConversationAssignment = model<IConversationAssignment>(
    'ConversationAssignment',
    ConversationAssignmentSchema,
);
export default ConversationAssignment;
