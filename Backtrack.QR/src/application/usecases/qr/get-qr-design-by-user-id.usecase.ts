import { Result, success } from '@/src/shared/core/result.js';
import { QrDesignRepository } from '@/src/application/repositories/qr-design.repository.js';
import { QrDesign } from '@/src/domain/entities/qr-design.entity.js';

type Deps = { qrDesignRepository: QrDesignRepository };

export const getQrDesignByUserIdUseCase = (deps: Deps) => async (userId: string): Promise<Result<QrDesign>> => {
  const existing = await deps.qrDesignRepository.findByUserId(userId);
  if (existing) return success(existing);

  const created = await deps.qrDesignRepository.createDefaultForUser(userId);
  return success(created);
};
