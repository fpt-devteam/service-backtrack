import { Request, Response } from 'express';
import { isSuccess } from '@/src/shared/core/result.js';
import { ok, fail, getHttpStatus } from '@/src/presentation/contracts/common/api-response.js';
import { getQrByUserId, getQrByPublicCode, updateQrNote, getQrDesignByUserId, updateQrDesignByUserId } from '@/src/infrastructure/container.js';
import { getHeader, HeaderNames } from '@/src/presentation/utils/http-headers.util.js';
import { ServerErrors } from '@/src/application/errors/server.error.js';
import { UpdateQrNoteRequest } from '@/src/presentation/contracts/qr/requests/update-qr-note.request.js';
import { UpdateQrDesignRequest } from '@/src/presentation/contracts/qr/requests/update-qr-design.request.js';
import { GetQrDesignResponse } from '@/src/presentation/contracts/qr/responses/get-qr-design.response.js';

export const getQrByUserIdAsync = async (req: Request, res: Response): Promise<void> => {
  const userId = getHeader(req, HeaderNames.AuthId);
  if (!userId) {
    res.status(500).json(fail(ServerErrors.MissingUserIdHeader));
    return;
  }
  const result = await getQrByUserId(userId);
  if (isSuccess(result))
    res.json(ok(result.value));
  else
    res.status(getHttpStatus(result.error)).json(fail(result.error));
};

export const getQrByPublicCodeAsync = async (req: Request, res: Response): Promise<void> => {
  const { publicCode } = req.params;
  const result = await getQrByPublicCode(publicCode);
  if (isSuccess(result))
    res.json(ok(result.value));
  else
    res.status(getHttpStatus(result.error)).json(fail(result.error));
};

export const updateQrNoteAsync = async (req: Request<{}, {}, UpdateQrNoteRequest>, res: Response): Promise<void> => {
  const userId = getHeader(req, HeaderNames.AuthId);
  if (!userId) {
    res.status(500).json(fail(ServerErrors.MissingUserIdHeader));
    return;
  }
  const { note } = req.body;
  const result = await updateQrNote(userId, note);
  if (isSuccess(result))
    res.json(ok(result.value));
  else
    res.status(getHttpStatus(result.error)).json(fail(result.error));
};

export const getQrDesignByUserIdAsync = async (req: Request, res: Response): Promise<void> => {
  const userId = getHeader(req, HeaderNames.AuthId);

  if (!userId) {
    res.status(500).json(fail(ServerErrors.MissingUserIdHeader));
    return;
  }

  const result = await getQrDesignByUserId(userId);

  if (isSuccess(result)) {
    res.json(ok<GetQrDesignResponse>(result.value));
  } else {
    res.status(getHttpStatus(result.error)).json(fail(result.error));
  }
};

export const updateQrDesignByUserIdAsync = async (req: Request<{}, {}, UpdateQrDesignRequest>, res: Response): Promise<void> => {
  const userId = getHeader(req, HeaderNames.AuthId);

  if (!userId) {
    res.status(500).json(fail(ServerErrors.MissingUserIdHeader));
    return;
  }

  const existingDesign = await getQrDesignByUserId(userId);
  
  if (isSuccess(existingDesign) && !existingDesign.value) {
    res.status(404).json(fail(ServerErrors.QrDesignNotFound));
    return;
  }

  const result = await updateQrDesignByUserId(userId, req.body);

  if (isSuccess(result)) {
    res.json(ok<GetQrDesignResponse>(result.value));
  } else {
    res.status(getHttpStatus(result.error)).json(fail(result.error));
  }
};