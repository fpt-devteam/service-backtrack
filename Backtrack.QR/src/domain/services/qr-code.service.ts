import { Item, QrCodeModel } from '@/src/infrastructure/database/models/qr-code.models.js';
import { qrCodeRepository } from '@/src/infrastructure/repositories/qr-code.repository.js';
import { toObjectIdOrNull } from '@/src/shared/utils/mongoes.object-id.util.js';
import { generatePublicCode } from '@/src/shared/utils/qr-code-generator.js';
import { Result, success, failure } from '@/src/shared/utils/result.js';
import { userRepository } from '@/src/infrastructure/repositories/user.repository.js';
import { QrCodeErrors } from '@/src/shared/errors/catalog/qr-code.error.js';
import { UserErrors } from '@/src/shared/errors/catalog/user.error.js';
import type { CreateItemRequest, CreateQrCodeRequest, UpdateItemRequest } from '@/src/shared/contracts/qr-code/qr-code.request.js';
import type { QrCodeResponse, QrCodeStatusResponse, QrCodeWithOwnerResponse } from '@/src/shared/contracts/qr-code/qr-code.response.js';
import { toQrCodeResponse, toQrCodeWithOwnerResponse } from '@/src/shared/contracts/qr-code/qr-code.mapper.js';
import { createPagedResponse, type PagedResponse } from '@/src/shared/contracts/common/pagination.js';
import QRCode from 'qrcode';
import { PUBLIC_QR_CODE_URL_PREFIX, QR_ERROR_CORRECTION_LEVEL, QR_MARGIN, QR_TYPE, QR_WIDTH } from '@/src/shared/configs/constants.js';
import { env } from '@/src/shared/configs/env.js';
import { stripUndefined } from '@/src/shared/utils/object.js';
import { orderRepository } from '@/src/infrastructure/repositories/order.repository.js';
import { OrderStatus } from '@/src/infrastructure/database/models/order.model.js';
import type { QrGenerationRequestedEvent } from '@/src/shared/contracts/events/order-events.js';
import * as logger from '@/src/shared/utils/logger.js';
import pLimit from 'p-limit';


export const createAsync = async (
  request: CreateQrCodeRequest,
  ownerId: string
): Promise<Result<QrCodeResponse>> => {
  let publicCode = "";
  for (let attempts = 0; attempts < env.QR_RETRY_ATTEMPTS; attempts++) {
    publicCode = generatePublicCode();

    const exists = await qrCodeRepository.existsByPublicCodeAsync(publicCode);
    if (!exists) break;

    if (attempts === env.QR_RETRY_ATTEMPTS - 1) {
      return failure({
        kind: "Internal",
        code: "QrCodeGenerationFailed",
        message: "Failed to generate unique QR code after maximum retries"
      });
    }
  }

  const qrCode = new QrCodeModel({
    publicCode,
    ownerId,
    item: {
      name: request.item.name,
      description: request.item.description,
      imageUrls: request.item.imageUrls || [],
    },
    linkedAt: new Date(),
  });

  const created = await qrCodeRepository.create(qrCode);
  return success(toQrCodeResponse(created));
};

export const getAllAsync = async (
  ownerId: string,
  page: number,
  pageSize: number
): Promise<Result<PagedResponse<QrCodeResponse>>> => {
  const offset = (page - 1) * pageSize;
  const { qrCodes, totalCount } = await qrCodeRepository.getAllAsync(ownerId, offset, pageSize);

  const qrCodeResponses = qrCodes.map(qrCode => toQrCodeResponse(qrCode));
  const pagedResponse = createPagedResponse(qrCodeResponses, page, pageSize, totalCount);

  return success(pagedResponse);
};

export const getByIdAsync = async (
  id: string
): Promise<Result<QrCodeWithOwnerResponse>> => {
  const objectId = toObjectIdOrNull(id);

  if (objectId === null) {
    return failure(QrCodeErrors.NotFound);
  }

  const qrCode = await qrCodeRepository.findById(objectId);

  if (qrCode === null) {
    return failure(QrCodeErrors.NotFound);
  }

  const owner = await userRepository.findById(qrCode.ownerId);
  if (owner === null) {
    return failure(UserErrors.NotFound);
  }

  return success(toQrCodeWithOwnerResponse(qrCode, owner));
};

export const getByPublicCodeAsync = async (
  publicCode: string
): Promise<Result<QrCodeWithOwnerResponse>> => {
  const { qrCode, owner } = await qrCodeRepository.getByPublicCodeAsync(publicCode);

  if (!qrCode || !owner) {
    return failure(QrCodeErrors.NotFound);
  }

  return success(toQrCodeWithOwnerResponse(qrCode, owner));
};

export const updateItemAsync = async (
  qrCodeId: string,
  request: UpdateItemRequest
): Promise<Result<QrCodeResponse>> => {
  const objectId = toObjectIdOrNull(qrCodeId);
  if (objectId === null) {
    return failure(QrCodeErrors.NotFound);
  }

  const patch = stripUndefined({
    name: request.name,
    description: request.description,
    imageUrls: request.imageUrls,
  });

  if (Object.keys(patch).length === 0) {
    return failure(QrCodeErrors.RequireAtLeastOneField);
  }
  const updated = await qrCodeRepository.updateItemAsync(objectId, patch);

  if (updated === null) {
    return failure(QrCodeErrors.NotFound);
  }

  return success(toQrCodeResponse(updated));
};

export const generateQrImage = async (
  publicCode: string
): Promise<Result<{ qrCodeImage: Buffer }>> => {
  try {
    const { qrCode } = await qrCodeRepository.getByPublicCodeAsync(publicCode);
    if (!qrCode) {
      return failure(QrCodeErrors.NotFound);
    }

    const qrUrl = `${env.WEB_CONSOLE_BACKTRACK}/${PUBLIC_QR_CODE_URL_PREFIX}/${publicCode}`;
    const qrCodeImage = await QRCode.toBuffer(qrUrl, {
      width: QR_WIDTH,
      margin: QR_MARGIN,
      errorCorrectionLevel: QR_ERROR_CORRECTION_LEVEL,
      color: {
        dark: '#000000',
        light: '#FFFFFF',
      },
      type: QR_TYPE
    });

    return success({ qrCodeImage });
  } catch (error) {
    return failure({
      kind: "Internal",
      code: "QrImageGenerationFailed",
      message: "Failed to generate QR code image",
      cause: error
    });
  }
};

export const activateQrCodeAsync = async (
  publicCode: string, userId: string, request: CreateItemRequest
): Promise<Result<QrCodeResponse>> => {
  const { qrCode, owner } = await qrCodeRepository.getByPublicCodeAsync(publicCode);

  if (!qrCode || !owner) {
    return failure(QrCodeErrors.NotFound);
  }
  if (qrCode.ownerId !== userId) {
    return failure(QrCodeErrors.Forbidden);
  }
  const item: Item = {
    name: request.name,
    description: request.description,
    imageUrls: request.imageUrls || [],
  };
  const updated = await qrCodeRepository.activateQrCodeAsync(qrCode._id, item);
  if (updated === null) {
    return failure(QrCodeErrors.NotFound);
  }
  return success(toQrCodeResponse(updated))
};

export const createPhysicalQrCodeAsync = async (
  ownerId: string
): Promise<Result<QrCodeResponse>> => {
  let publicCode = "";
  for (let attempts = 0; attempts < env.QR_RETRY_ATTEMPTS; attempts++) {
    publicCode = generatePublicCode();
    const exists = await qrCodeRepository.existsByPublicCodeAsync(publicCode);
    if (!exists) break;
    if (attempts === env.QR_RETRY_ATTEMPTS - 1) {
      return failure({
        kind: "Internal",
        code: "QrCodeGenerationFailed",
        message: "Failed to generate unique QR code after maximum retries"
      });
    }
  }
  const qrCode = new QrCodeModel({
    publicCode,
    ownerId,
    item: null,
    linkedAt: null,
  });
  const created = await qrCodeRepository.create(qrCode);
  return success(toQrCodeResponse(created));
};

export interface BatchQrGenerationResult {
  success: boolean;
  generatedCount: number;
  failedCount: number;
  publicCodes: string[];
}

export const processBatchQrGenerationAsync = async (
  event: QrGenerationRequestedEvent
): Promise<BatchQrGenerationResult> => {
  const { code, userId, qrCount, packageName } = event;

  logger.info('Processing QR generation request', {
    code,
    userId,
    qrCount,
    packageName,
  });

  const order = await orderRepository.findByCode(code);
  if (!order) {
    logger.error('Order not found for QR generation', { code });
    throw new Error(`Order not found: ${code}`);
  }

  if (order.status !== OrderStatus.PAID) {
    logger.warn('Order is not in PAID status, skipping QR generation', {
      code,
      currentStatus: order.status,
    });
    return {
      success: false,
      generatedCount: 0,
      failedCount: 0,
      publicCodes: [],
    };
  }

  const result = await generateQrCodesBatchAsync(userId, qrCount, code);

  if (result.generatedCount === qrCount) {
    logger.info('All QR codes generated successfully', {
      code,
      totalGenerated: result.generatedCount,
    });
  } else if (result.generatedCount > 0) {
    logger.warn('Partial QR code generation', {
      code,
      successCount: result.generatedCount,
      failedCount: result.failedCount,
    });
  } else {
    throw new Error(`All QR code generation failed for order ${code}`);
  }

  return result;
};

const generateQrCodesBatchAsync = async (
  userId: string,
  totalCount: number,
  code: string
): Promise<BatchQrGenerationResult> => {
  const publicCodes: string[] = [];
  let failedCount = 0;

  const limit = pLimit(env.QR_CONCURRENCY);

  for (let i = 0; i < totalCount; i += env.QR_BATCH_SIZE) {
    const batchSize = Math.min(env.QR_BATCH_SIZE, totalCount - i);

    const batchPromises = Array.from({ length: batchSize }, (_, j) =>
      limit(() => createSinglePhysicalQrCodeAsync(userId, code, i + j + 1, totalCount))
    );

    const results = await Promise.allSettled(batchPromises);

    for (const result of results) {
      if (result.status === 'fulfilled' && result.value) {
        publicCodes.push(result.value);
      } else {
        failedCount++;
      }
    }
  }

  return {
    success: failedCount === 0,
    generatedCount: publicCodes.length,
    failedCount,
    publicCodes,
  };
};

const createSinglePhysicalQrCodeAsync = async (
  userId: string,
  orderCode: string,
  index: number,
  total: number
): Promise<string | null> => {
  const maxRetries = env.QR_RETRY_ATTEMPTS;

  for (let attempt = 0; attempt < maxRetries; attempt++) {
    try {
      const publicCode = await generateUniquePublicCodeAsync();

      const qrCode = new QrCodeModel({
        publicCode,
        ownerId: userId,
        item: null,
        linkedAt: null,
      });

      await qrCodeRepository.create(qrCode);

      logger.debug(`Generated QR code ${index}/${total}`, {
        orderCode,
        publicCode,
      });

      return publicCode;
    } catch (error) {
      const isDuplicateKeyError = error instanceof Error &&
        error.message.includes('E11000') &&
        error.message.includes('publicCode');

      if (isDuplicateKeyError && attempt < maxRetries - 1) {
        logger.warn(`Duplicate publicCode collision, retrying (attempt ${attempt + 1}/${maxRetries})`, {
          orderCode,
          index,
        });
        continue;
      }

      logger.error(`Failed to generate QR code ${index}/${total}`, {
        orderCode,
        error: String(error),
        attempt: attempt + 1,
      });
      return null;
    }
  }

  return null;
};

const generateUniquePublicCodeAsync = async (): Promise<string> => {
  for (let attempts = 0; attempts < env.QR_RETRY_ATTEMPTS; attempts++) {
    const publicCode = generatePublicCode();
    const exists = await qrCodeRepository.existsByPublicCodeAsync(publicCode);

    if (!exists) {
      return publicCode;
    }
  }

  throw new Error('Failed to generate unique public code after maximum retries');
};