import { z } from "zod";

export type CreateQrCodeRequest = {
  item: {
    name: string;
    description: string;
    imageUrls?: string[];
  };
};

export const CreateQrCodeRequestSchema = z.object({
  item: z.object({
    name: z.string().trim().min(1, "name is required").max(100),
    description: z.string().min(1, "description is required").max(500),
    imageUrls: z.array(z.string().trim().min(1, "url is required")).min(1, "at least one image url is required").max(5),
  }),
}).strict();


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