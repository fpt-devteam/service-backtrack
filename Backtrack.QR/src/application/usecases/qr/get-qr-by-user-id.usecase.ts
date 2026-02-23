import { Result, success, failure } from '@/src/shared/core/result.js';
import { QrErrors } from '@/src/application/errors/qr.error.js';
import { QrRepository } from '@/src/application/repositories/qr.repository.js';
import { Qr } from '@/src/domain/entities/qr.entity.js';

type Deps = { qrRepository: QrRepository };

export const getQrByUserIdUseCase = (deps: Deps) => async (userId: string): Promise<Result<Qr>> => {
  const qr = await deps.qrRepository.findByUserId(userId);
  if (!qr) return failure(QrErrors.NotFound);
  return success(qr);
};