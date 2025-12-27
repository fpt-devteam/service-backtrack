import { Router } from 'express';
import asyncHandler from "express-async-handler";

import * as qrCodeController from '@/src/presentation/controllers/qr-code.controller.js';

const router = Router();

// Public endpoint - no auth required
router.get('/public/qr-code/:publicCode', asyncHandler(qrCodeController.getByPublicCodeAsync));

// Protected endpoints - auth required
router.post('/qr-codes', asyncHandler(qrCodeController.createAsync));
router.get('/qr-codes', asyncHandler(qrCodeController.getAllAsync));
router.get('/qr-codes/:id', asyncHandler(qrCodeController.getByIdAsync));
router.put('/qr-codes/:id/item', asyncHandler(qrCodeController.updateItemAsync));

export default router;
