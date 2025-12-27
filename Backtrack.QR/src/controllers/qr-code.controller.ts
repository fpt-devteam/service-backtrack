import { Request, Response } from 'express';
import * as qrCodeService from '@/src/services/qr-code.service.js';
import { isSuccess } from '@/src/utils/result.js';
import { ok, fail, getHttpStatus } from '@/src/contracts/common/api-response.js';
import * as logger from '@/src/utils/logger.js';
import type { CreateQrCodeRequest, UpdateItemRequest } from '@/src/contracts/qr-code/qr-code.request.js';
import { HEADER_AUTH_ID } from '@/src/utils/headers.js';
import { createPagedResponse, sanitizePage, sanitizePageSize } from '@/src/contracts/common/pagination.js';
import { toQrCodeResponse, toQrCodeWithOwnerResponse } from '@/src/contracts/qr-code/qr-code.mapper.js';

/**
 * Create a new digital QR code with linked item
 * POST /qr-codes
 */
export const createAsync = async (req: Request, res: Response) => {
    const request: CreateQrCodeRequest = req.body;
    const correlationId = req.correlationId || 'unknown';
    const ownerId = req.headers[HEADER_AUTH_ID] as string;

    const result = await qrCodeService.createAsync(
        {
            name: request.item.name,
            description: request.item.description,
            imageUrls: request.item.imageUrls || [],
        },
        ownerId
    );

    if (isSuccess(result)) {
        const response = toQrCodeResponse(result.value);
        res.status(201).json(ok(response));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to create QR code', {
            error: result.error,
            correlationId
        });
        res.status(status).json(fail(
            result.error.code,
            result.error.message
        ));
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
        const { qrCodes, totalCount } = result.value;
        const pagedResponse = createPagedResponse(
            qrCodes.map((qrCode) => toQrCodeResponse(qrCode)),
            page,
            pageSize,
            totalCount
        );

        res.json(ok(pagedResponse));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to get QR codes', {
            ownerId,
            error: result.error,
        });
        res.status(status).json(fail(
            result.error.code,
            result.error.message
        ));
    }
};

/**
 * Get QR code by ID
 * GET /qr-codes/:id
 */
export const getByIdAsync = async (req: Request, res: Response) => {
    const { id } = req.params;

    console.log(`Received request to get QR code by ID: ${id}`);

    const result = await qrCodeService.getByIdAsync(id);

    if (isSuccess(result)) {
        const response = toQrCodeWithOwnerResponse(result.value.qrCode, result.value.owner);
        res.json(ok(response));
    } else {
        const status = getHttpStatus(result.error);
        res.status(status).json(fail(
            result.error.code,
            result.error.message
        ));
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
        res.json(ok(toQrCodeWithOwnerResponse(result.value.qrCode, result.value.owner)));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('QR code not found by public code', {
            publicCode,
            error: result.error,
            correlationId
        });
        res.status(status).json(fail(
            result.error.code,
            result.error.message
        ));
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

    const result = await qrCodeService.updateItemAsync(id, {
        name: request.name,
        description: request.description,
        imageUrls: request.imageUrls || [],
    });

    if (isSuccess(result)) {
        const response = toQrCodeResponse(result.value);
        res.json(ok(response));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to update QR code item', {
            qrCodeId: id,
            error: result.error,
            correlationId
        });
        res.status(status).json(fail(
            result.error.code,
            result.error.message
        ));
    }
};
