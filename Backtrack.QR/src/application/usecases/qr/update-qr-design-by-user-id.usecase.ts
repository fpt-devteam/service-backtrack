import { failure, Result, success } from '@/src/shared/core/result.js';
import { QrDesignRepository, UpdateQrDesignFields } from '@/src/application/repositories/qr-design.repository.js';
import { QrDesign } from '@/src/domain/entities/qr-design.entity.js';
import { ServerErrors } from '../../errors/server.error.js';

type Deps = { qrDesignRepository: QrDesignRepository };

export const updateQrDesignByUserIdUseCase = (deps: Deps) => async (
  userId: string,
  fields: UpdateQrDesignFields
): Promise<Result<QrDesign>> => {
  let updated = await deps.qrDesignRepository.updateByUserId(userId, fields);
  if (!updated) return failure(ServerErrors.UnexpectedError);
  return success(updated);
};
