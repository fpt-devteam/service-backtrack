import { model, Schema, Types } from "mongoose";
import { UUID } from "node:crypto";

export interface QrDocument extends Document {
  _id: Types.ObjectId;
  userId: string;
  publicCode: UUID;
  createdAt: Date;
  updatedAt: Date;
}

const qrSchema = new Schema<QrDocument>(
  {
    userId: { type: String, required: true },
    publicCode: { type: String, required: true, unique: true },
  },
  { timestamps: true }
);

export const QrModel = model<QrDocument>('Qr', qrSchema);