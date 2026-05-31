import { Schema, model } from 'mongoose';
import { ConversationStatus, ISupportConversation } from './interfaces/support-conversation.interface';



const ConversationSchema = new Schema<ISupportConversation>(
	{
		lastMessageAt: { type: Date },
		lastMessageContent: { type: String, default: null },
		senderId: { type: String, default: null },
		staffAssignId: { type: String, default: null, index: true },
		orgId: {
			type: String,
			required: true,
			index: true
		},
		orgName: { type: String, default: null },
		orgSlug: { type: String, default: null },
		orgLogoUrl: { type: String, default: null },
		status: {
			type: String,
			enum: Object.values(ConversationStatus),
			required: true,
			index: true
		},
		supportFormData: {
			postId: { type: String, default: null },
			category: { type: String, required: true },
			subCategoryId: { type: String, required: true },
			itemName: { type: String, required: true },
			color: { type: String, required: true },
			additionalDetails: { type: String, default: null },
			imageUrls: { type: [String], default: null },
			lostLocation: { type: String, default: null },
			eventTime: { type: Date, default: null },
			contactName: { type: String, default: null },
			contactPhone: { type: String, default: null },
			contactEmail: { type: String, default: null },
		},
		resolvedAt: { type: Date, default: null },
		verifiedAt: { type: Date, default: null },
		deletedAt: { type: Date, default: null },
	},
	{
		timestamps: true,
		minimize: false,
	},
);

ConversationSchema.index({ orgId: 1, lastMessageAt: -1 });
ConversationSchema.index({ senderId: 1, lastMessageAt: -1 });
ConversationSchema.index({ deletedAt: 1 });
ConversationSchema.index({ staffAssignId: 1, lastMessageAt: -1 });

const SupportConversation = model<ISupportConversation>('SupportConversation', ConversationSchema);
export default SupportConversation;
