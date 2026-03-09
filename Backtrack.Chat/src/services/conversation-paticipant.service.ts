import { CreateConversationRequest } from '@/dtos/conversation/conversation.request';
import { ConversationParticipantRole, ConversationType } from '@/models';
import ConversationParticipant from '@/models/conversation-participant';
import mongoose from 'mongoose';

export const createConvParticipants = async(
	conversationId: mongoose.Types.ObjectId,
	data: CreateConversationRequest,
	userId: string,
) => {
	let participants;
	if (data.type === ConversationType.ORGANIZATION) {
		participants = [
			{
				conversationId,
				memberId: userId,
				role: ConversationParticipantRole.CUSTOMER,
			},
			{
				conversationId,
				orgId: data.orgId,
				role: ConversationParticipantRole.STAFF,
			},
		];
	} else {
		participants = [
			{
				conversationId,
				memberId: userId,
				role: ConversationParticipantRole.CUSTOMER,
			},
			{
				conversationId,
				memberId: data.memberId,
				role: ConversationParticipantRole.CUSTOMER,
			},
		];
	}
	await ConversationParticipant.insertMany(participants);
};

export const updateUnreadCount = async (conversationId: string, senderId: string, increment = 1): Promise<void> => {
    await ConversationParticipant.updateMany(
        { conversationId, memberId: { $ne: senderId } },
        { $inc: { unreadCount: increment } }
    );

};

export const resetUnreadCount = async (conversationId: string, memberId: string): Promise<void> => {
    await ConversationParticipant.updateOne(
        { conversationId, memberId },
        { $set: { unreadCount: 0 } }
    );
};