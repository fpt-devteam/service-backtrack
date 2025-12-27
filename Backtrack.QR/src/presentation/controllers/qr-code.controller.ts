import { Request, Response } from 'express';
import * as qrCodeService from '@/src/domain/services/qr-code.service.js';
import { isSuccess } from '@/src/shared/utils/result.js';
import { ok, fail, getHttpStatus } from '@/src/shared/contracts/common/api-response.js';
import * as logger from '@/src/shared/utils/logger.js';
import type { CreateQrCodeRequest, UpdateItemRequest } from '@/src/shared/contracts/qr-code/qr-code.request.js';
import { HEADER_AUTH_ID } from '@/src/shared/utils/headers.js';
import { sanitizePage, sanitizePageSize } from '@/src/shared/contracts/common/pagination.js';

/**
 * Create a new digital QR code with linked item
 * POST /qr-codes
 */
export const createAsync = async (req: Request, res: Response) => {
    const request: CreateQrCodeRequest = req.body;
    const correlationId = req.correlationId || 'unknown';
    const ownerId = req.headers[HEADER_AUTH_ID] as string;

    const result = await qrCodeService.createAsync(request, ownerId);

    if (isSuccess(result)) {
        res.status(201).json(ok(result.value));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to create QR code', {
            error: result.error,
            correlationId
        });
        res.status(status).json(fail(result.error.code, result.error.message));
    }
};

/**
 * Get all QR codes for authenticated user
 * GET /qr-codes
 */
export const getAllAsync = async (req: Request, res: Response) => {
    const ownerId = req.headers[HEADER_AUTH_ID] as string;
    const page = sanitizePage(req.query.page);
    const pageSize = sanitizePageSize(req.query.pageSize);

    const result = await qrCodeService.getAllAsync(ownerId, page, pageSize);

    if (isSuccess(result)) {
        res.json(ok(result.value));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to get QR codes', {
            ownerId,
            error: result.error,
        });
        res.status(status).json(fail(result.error.code, result.error.message));
    }
};

/**
 * Get QR code by ID
 * GET /qr-codes/:id
 */
export const getByIdAsync = async (req: Request, res: Response) => {
    const { id } = req.params;

    const result = await qrCodeService.getByIdAsync(id);

    if (isSuccess(result)) {
        res.json(ok(result.value));
    } else {
        const status = getHttpStatus(result.error);
        res.status(status).json(fail(result.error.code, result.error.message));
    }
};

/**
 * Get QR code by public code (public endpoint, no auth)
 * GET /public/qr-code/:publicCode
 */
export const getByPublicCodeAsync = async (req: Request, res: Response) => {
    const { publicCode } = req.params;
    const correlationId = req.correlationId || 'unknown';

    const result = await qrCodeService.getByPublicCodeAsync(publicCode);

    if (isSuccess(result)) {
        res.json(ok(result.value));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('QR code not found by public code', {
            publicCode,
            error: result.error,
            correlationId
        });
        res.status(status).json(fail(result.error.code, result.error.message));
    }
};

/**
 * Update item linked to QR code
 * PUT /qr-codes/:id/item
 */
export const updateItemAsync = async (req: Request, res: Response) => {
    const { id } = req.params;
    const request: UpdateItemRequest = req.body;
    const correlationId = req.correlationId || 'unknown';

    const result = await qrCodeService.updateItemAsync(id, request);

    if (isSuccess(result)) {
        res.json(ok(result.value));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to update QR code item', {
            qrCodeId: id,
            error: result.error,
            correlationId
        });
        res.status(status).json(fail(result.error.code, result.error.message));
    }
};
