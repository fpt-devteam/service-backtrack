import { z } from 'zod';

export type UpdateQrNoteRequest = {
  note: string;
};

export const UpdateQrNoteRequestSchema = z.object({
  note: z.string().trim().min(1, 'note is required').max(500, 'note must be at most 500 characters'),
}).strict();
