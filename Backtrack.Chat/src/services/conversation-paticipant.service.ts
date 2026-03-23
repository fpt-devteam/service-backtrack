import { ConversationParticipantRole } from '@/models';
import ConversationParticipant from '@/models/conversation-participant';

export const createDirectConvParticipants = async(
	conversationId: string,
	memberId: string,
	userId: string,
) => {	
	const participants = [
		{
			conversationId,
			memberId: userId,
			role: ConversationParticipantRole.CUSTOMER,
		},
		{
			conversationId,
			memberId: memberId,
			role: ConversationParticipantRole.CUSTOMER,
		},
	];
	await ConversationParticipant.insertMany(participants);
};

export const createOrgConvParticipants = async(
	conversationId: string,
	role: ConversationParticipantRole,
	userId: string,
	data?: any
) => {	
	const participant =
		{
			conversationId,
			memberId: userId,
			role: role,
			...data
		};
	await ConversationParticipant.insertOne(participant);
};

export const unassignConversationParticipant = async (conversationId: string, role: ConversationParticipantRole) => {
    await ConversationParticipant.findOneAndUpdate(
        { conversationId, role },
        { isActive: false },
        { new: true },
    );
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