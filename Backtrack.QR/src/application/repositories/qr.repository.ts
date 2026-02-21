import { Qr } from '@/src/domain/entities/qr.entity.js';
import { UserEnsureExistEvent } from '@/src/infrastructure/events/user-events.js';
import { UUID } from 'node:crypto';

export type QrRepository = {
  findById: (id: string) => Promise<Qr | null>;
  findByPublicCode: (publicCode: UUID) => Promise<Qr | null>;
  findByUserId: (userId: string) => Promise<Qr | null>;
  create: (qr: Omit<Qr, 'id' | 'createdAt' | 'updatedAt'>) => Promise<Qr>;
  ensureExist: (user: UserEnsureExistEvent) => Promise<Qr>;
};