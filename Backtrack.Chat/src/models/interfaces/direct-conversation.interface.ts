
export interface HandoverUserResult {
    id: string;
    email: string | null;
    displayName: string | null;
    avatarUrl: string | null;
    phone: string | null;
    showEmail: boolean;
    showPhone: boolean;
    globalRole: string;
    status: string;
}

export interface HandoverPostResult {
    id: string;
    postType: string;
    status: string;
    category: string;
    imageUrls: string[];
    displayAddress: string | null;
    eventTime: string;
    createdAt: string;
}

export interface Handover {
    id: string;
    finder: HandoverUserResult;
    owner: HandoverUserResult | null;
    finderPost: HandoverPostResult | null;
    ownerPost: HandoverPostResult | null;
    /** 'Draft' | 'Active' | 'Confirmed' | 'Rejected' | 'Expired' */
    status: string;
    /** 'Finder' | 'Owner' | null */
    activatedByRole: string | null;
    confirmedAt: string | null;
    expiresAt: string;
    createdAt: string;
}

export interface IDirectConversation {
    id: string;
	lastMessageAt: Date | null;
	lastMessageContent: string | null;
	senderId: string | null;
	handover: Handover | null;
	createdAt: Date;
	updatedAt: Date;
	deletedAt: Date | null;
}
