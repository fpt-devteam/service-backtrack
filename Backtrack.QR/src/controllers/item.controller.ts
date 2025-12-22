import { Request, Response } from 'express';
import * as ItemService from '@/src/services/item.service.js';
import { isSuccess } from '@/src/utils/result.js';
import { ok, fail, getHttpStatus } from '@/src/contracts/common/api-response.js';
import { toCreateItemInput, toItemResponse } from '../contracts/item/mapper.js';
import * as logger from '@/src/utils/logger.js';
import type { CreateItemRequest } from '@/src/contracts/item/request.js';
import { HEADER_AUTH_ID } from '@/src/utils/headers.js';

export const getById = async (req: Request, res: Response) => {
    const { id } = req.params;
    const correlationId = req.correlationId || 'unknown';

    logger.debug('Getting item by ID', { id, correlationId });

    const result = await ItemService.getByIdAsync(id);

    if (isSuccess(result)) {
        const response = toItemResponse(result.value);
        logger.info('Item retrieved successfully', { itemId: id, correlationId });
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
    const userId = req.headers[HEADER_AUTH_ID] as string;

    const input = toCreateItemInput(request, userId);

    const result = await ItemService.createAsync(input);

    if (isSuccess(result)) {
        const response = toItemResponse(result.value);
        res.status(201).json(ok(response));
    } else {
        const status = getHttpStatus(result.error);
        logger.warn('Failed to create item', {
            name: request.name,
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
