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

const MAX_RETRIES = 5;

export const createAsync = async (
  request: CreateQrCodeRequest,
  ownerId: string
): Promise<Result<QrCodeResponse>> => {
  let publicCode = "";
  for (let attempts = 0; attempts < MAX_RETRIES; attempts++) {
    publicCode = generatePublicCode();

    const exists = await qrCodeRepository.existsByPublicCodeAsync(publicCode);
    if (!exists) break;

    if (attempts === MAX_RETRIES - 1) {
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
  for (let attempts = 0; attempts < MAX_RETRIES; attempts++) {
    publicCode = generatePublicCode();
    const exists = await qrCodeRepository.existsByPublicCodeAsync(publicCode);
    if (!exists) break; 
    if (attempts === MAX_RETRIES - 1) {
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