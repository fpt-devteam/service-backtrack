import { Request, Response } from 'express';
import * as packageService from '@/src/domain/services/package.service.js';
import { CreatePackageRequest, UpdatePackageRequest } from '@/src/shared/contracts/package/package.request.js';
import { isFailure } from '@/src/shared/utils/result.js';
import { getHttpStatusCode } from '@/src/presentation/utils/http-status.js';

export const createAsync = async (req: Request, res: Response): Promise<void> => {
    const request: CreatePackageRequest = req.body;
    const correlationId = (req as any).correlationId || 'unknown';

    const result = await packageService.createAsync(request);

    if (isFailure(result)) {
        res.status(getHttpStatusCode(result.error)).json({
            error: result.error,
            correlationId
        });
        return;
    }

    res.status(201).json({
        data: result.value,
        correlationId
    });
};

export const getAllAsync = async (req: Request, res: Response): Promise<void> => {
    const correlationId = (req as any).correlationId || 'unknown';

    const result = await packageService.findAllAsync();

    if (isFailure(result)) {
        res.status(500).json({
            error: result.error,
            correlationId
        });
        return;
    }

    res.status(200).json({
        data: result.value,
        correlationId
    });
};

export const getByIdAsync = async (req: Request, res: Response): Promise<void> => {
    const { id } = req.params;
    const correlationId = (req as any).correlationId || 'unknown';

    const result = await packageService.findByIdAsync(id);

    if (isFailure(result)) {
        res.status(getHttpStatusCode(result.error)).json({
            error: result.error,
            correlationId
        });
        return;
    }

    res.status(200).json({
        data: result.value,
        correlationId
    });
};

export const updateAsync = async (req: Request, res: Response): Promise<void> => {
    const { id } = req.params;
    const request: CreatePackageRequest = req.body;
    const correlationId = (req as any).correlationId || 'unknown';

    const result = await packageService.updateByIdAsync(id, request);

    if (isFailure(result)) {
        res.status(getHttpStatusCode(result.error)).json({
            error: result.error,
            correlationId
        });
        return;
    }

    res.status(200).json({
        data: result.value,
        correlationId
    });
};

export const deleteAsync = async (req: Request, res: Response): Promise<void> => {
    const { id } = req.params;
    const correlationId = (req as any).correlationId || 'unknown';

    const result = await packageService.deleteByIdAsync(id);

    if (isFailure(result)) {
        res.status(getHttpStatusCode(result.error)).json({
            error: result.error,
            correlationId
        });
        return;
    }

    res.status(204).send();
};
