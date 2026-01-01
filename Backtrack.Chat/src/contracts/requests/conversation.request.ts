
export interface CreateConversationInput {
  creatorId: string;
  partnerId: string;

  creatorKeyName: string;
  partnerKeyName: string;

  customAvatarUrl?: string | null;
}

export interface ModifyConversationParticipantNicknameInput {
  targetUserId: string;
  newNickname: string;
}