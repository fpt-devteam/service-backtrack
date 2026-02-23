import { UUID } from "node:crypto";

export type Qr = {
  id: string;
  userId: string;
  publicCode: UUID;
  createdAt: Date;
  updatedAt: Date;
}