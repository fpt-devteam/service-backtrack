import { ErrorCodes } from '@src/common/errors';
import { participantRepository } from '@src/repositories';

export class ConversationParticipantService {
  public async modifyNickname(
    conversationId: string,
    memberId: string,
    targetUserId: string,
    newNickname: string,
  ): Promise<void> {
    const isMemberParticipant = await participantRepository.isParticipant(
      conversationId,
      memberId,
    );  
    if (!isMemberParticipant) {
      throw ErrorCodes.NotParticipant;
    }

    const isTargetParticipant = await participantRepository.isParticipant(
      conversationId,
      targetUserId,
    );
    if (!isTargetParticipant) {
      throw ErrorCodes.NotParticipant;
    }

    await participantRepository.setNickname(
      conversationId,
      targetUserId,
      newNickname,
    );
  }
}

export default new ConversationParticipantService();
