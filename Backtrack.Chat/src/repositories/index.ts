import { ConversationRepository } from './conversation.repository';
import { 
  ConversationParticipantRepository,
} from './conversation-participant.repository';
import { MessageRepository } from './message.repository';
import userRepository from './user.repository';

export const conversationRepository = new ConversationRepository();
export const participantRepository = new ConversationParticipantRepository();
export const messageRepository = new MessageRepository();
export { userRepository };
