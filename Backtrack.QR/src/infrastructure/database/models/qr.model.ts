import { model, Schema, Types } from "mongoose";
import { UUID } from "node:crypto";
import { DEFAULT_QR_NOTE } from "@/src/domain/entities/qr.entity.js";

export interface QrDocument extends Document {
  _id: Types.ObjectId;
  userId: string;
  publicCode: UUID;
  note: string;
  createdAt: Date;
  updatedAt: Date;
}

const qrSchema = new Schema<QrDocument>(
  {
    userId: { type: String, required: true },
    publicCode: { type: String, required: true, unique: true },
    note: { type: String, required: true, default: DEFAULT_QR_NOTE },
  },
  { timestamps: true }
);

export const QrModel = model<QrDocument>('Qr', qrSchema);