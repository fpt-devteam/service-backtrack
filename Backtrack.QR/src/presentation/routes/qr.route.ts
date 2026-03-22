import { Router } from 'express';
import asyncHandler from "express-async-handler";

import * as QrController from '@/src/presentation/controllers/qr.controller.js';
import { validateBody } from '@/src/presentation/middlewares/validation.middleware.js';
import { UpdateQrNoteRequestSchema } from '@/src/presentation/contracts/qr/requests/update-qr-note.request.js';

const router = Router();

router.get('/public/:publicCode', asyncHandler(QrController.getQrByPublicCodeAsync));
router.get('/me', asyncHandler(QrController.getQrByUserIdAsync));
router.get('/me/design', asyncHandler(QrController.getQrDesignByUserIdAsync));
router.put('/me/design', asyncHandler(QrController.updateQrDesignByUserIdAsync));
router.patch('/me/note', validateBody(UpdateQrNoteRequestSchema), asyncHandler(QrController.updateQrNoteAsync));

export default router;