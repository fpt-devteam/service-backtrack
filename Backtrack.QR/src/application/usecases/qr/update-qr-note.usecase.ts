import { Result, success, failure } from '@/src/shared/core/result.js';
import { QrErrors } from '@/src/application/errors/qr.error.js';
import { QrRepository } from '@/src/application/repositories/qr.repository.js';
import { Qr } from '@/src/domain/entities/qr.entity.js';

type Deps = { qrRepository: QrRepository };

export const updateQrNoteUseCase = (deps: Deps) => async (userId: string, note: string): Promise<Result<Qr>> => {
  const qr = await deps.qrRepository.updateByUserId(userId, { note });
  if (!qr) return failure(QrErrors.NotFound);
  return success(qr);
};
