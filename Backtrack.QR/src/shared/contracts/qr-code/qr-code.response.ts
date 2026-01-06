
export type ItemResponse = {
    name: string;
    description: string;
    imageUrls: string[];
};

export type UserResponse = {
    id: string;
    email: string;
    displayName?: string | null;
};

export type QrCodeResponse = {
    qrCode: {
        id: string;
        publicCode: string;
        linkedAt?: string | null;
        createdAt: string;
    };
    item?: ItemResponse | null;
    ownerId: string;
};

export type QrCodeStatusResponse = {
    status: boolean;
    qrCode?: {
        id: string;
        publicCode: string;
        linkedAt?: string | null;
        createdAt: string;
    };
    item?: ItemResponse | null;
    ownerId?: string;
};

export type QrCodeWithOwnerResponse = {
    qrCode: {
        id: string;
        publicCode: string;
        linkedAt?: string | null;
        createdAt: string;
    };
    item?: ItemResponse | null;
    owner: UserResponse;
};
