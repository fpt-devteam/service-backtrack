import { QrRepository } from '@/src/application/repositories/qr.repository.js';
import { QrModel } from '@/src/infrastructure/database/models/qr.model.js';
import { qrToDomain } from '@/src/infrastructure/database/mappers/qr.mapper.js';
import { Qr } from '@/src/domain/entities/qr.entity.js';
import { UserEnsureExistEvent } from '@/src/infrastructure/events/user-events.js';
import { generatePublicCode } from '@/src/application/utils/backtrack-code-generator.util.js';

export const createQrRepository = (): QrRepository => ({
  ensureExist: async (user: UserEnsureExistEvent) => {
    const existing = await QrModel.findOne({ userId: user.Id, deletedAt: null });
    if (existing) return qrToDomain(existing);
    const doc = await QrModel.create(
      {
        userId: user.Id,
        publicCode: await generateUniquePublicCode(),
      }
    );
    return qrToDomain(doc);
  },

  findById: async (id) => {
    const doc = await QrModel.findById(id);
    return doc ? qrToDomain(doc) : null;
  },

  findByUserId: async (userId) => {
    const doc = await QrModel.findOne({ userId, deletedAt: null });
    return doc ? qrToDomain(doc) : null;
  },

  findByPublicCode: async (publicCode) => {
    const doc = await QrModel.findOne({ publicCode });
    return doc ? qrToDomain(doc) : null;
  },

  create: async (qr: Omit<Qr, 'id' | 'createdAt' | 'updatedAt'>) => {
    const doc = await QrModel.create(qr);
    return qrToDomain(doc);
  }
});

const generateUniquePublicCode = async (): Promise<string> => {
  let unique = false;
  let publicCode: string;

  while (!unique) {
    publicCode = generatePublicCode();
    const existing = await QrModel.findOne({ publicCode });
    if (!existing) {
      unique = true;
    }
  }

  return publicCode!;
};

export const qrRepository = createQrRepository();