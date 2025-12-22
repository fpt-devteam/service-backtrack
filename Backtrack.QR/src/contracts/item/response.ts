export type ItemResponse = {
    id: string;
    name: string;
    description?: string;
    imageUrls?: string[];
    userId: string;
    createdAt: string;
    updatedAt: string | null;
};
