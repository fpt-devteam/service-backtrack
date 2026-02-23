import { Qr } from '@/src/domain/entities/qr.entity.js';
import { QrDocument } from '@/src/infrastructure/database/models/qr.model.js';

export const qrToDomain = (doc: QrDocument): Qr => ({
  id: doc._id.toString(),
  userId: doc.userId,
  publicCode: doc.publicCode,
  createdAt: doc.createdAt,
  updatedAt: doc.updatedAt,
});
