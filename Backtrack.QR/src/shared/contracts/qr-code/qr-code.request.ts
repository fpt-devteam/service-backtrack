export type CreateQrCodeRequest = {
  item: {
    name: string;
    description: string;
    imageUrls?: string[];
  };
};

export type UpdateItemRequest = {
  name?: string;
  description?: string;
  imageUrls?: string[];
};

export type CreateItemRequest = {
  name: string;
  description: string;
  imageUrls: string[];
};