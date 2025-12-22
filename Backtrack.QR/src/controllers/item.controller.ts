import { Request, Response } from 'express';
import * as ItemService from '@/src/services/item.service.js';
import { isSuccess } from '@/src/utils/result.js';
import { ok, fail, getHttpStatus } from '@/src/contracts/common/api-response.js';
import { toItemResponse } from '../contracts/item/item.mapper.js';
import * as logger from '@/src/utils/logger.js';
import type { CreateItemRequest } from '@/src/contracts/item/item.request.js';
import { HEADER_AUTH_ID } from '@/src/utils/headers.js';

export const getById = async (req: Request, res: Response) => {
    const { id } = req.params;
    const result = await ItemService.getByIdAsync(id);

    if (isSuccess(result)) {
        const response = toItemResponse(result.value.item, result.value.qrCode);
        res.json(ok(response));
    } else {
        const status = getHttpStatus(result.error);
        res.status(status).json(fail(
            result.error.code,
            result.error.message,
            result.error.details
        ));
    }
};

export const create = async (req: Request, res: Response) => {
    const request: CreateItemRequest = req.body;
    const correlationId = req.correlationId || 'unknown';
    const ownerId = req.headers[HEADER_AUTH_ID] as string;

    const result = await ItemService.createAsync(request, ownerId);

    if (isSuccess(result)) {
        const response = toItemResponse(result.value.item, result.value.qrCode);
        res.status(201).json(ok(response));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to create item with QR code', {
            itemName: request.name,
            error: result.error,
            correlationId
        });
        res.status(status).json(fail(
            result.error.code,
            result.error.message,
            result.error.details
        ));
    }
};
