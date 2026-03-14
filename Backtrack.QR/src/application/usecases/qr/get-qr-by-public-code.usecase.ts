import { Result, success, failure } from '@/src/shared/core/result.js';
import { QrErrors } from '@/src/application/errors/qr.error.js';
import { QrRepository } from '@/src/application/repositories/qr.repository.js';
import { Qr } from '@/src/domain/entities/qr.entity.js';

type Deps = { qrRepository: QrRepository };

export const getQrByPublicCodeUseCase = (deps: Deps) => async (publicCode: string): Promise<Result<Qr>> => {
  const qr = await deps.qrRepository.findByPublicCode(publicCode);
  if (!qr) return failure(QrErrors.NotFound);
  return success(qr);
};
