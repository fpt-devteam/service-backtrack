import { Router } from 'express';
import asyncHandler from "express-async-handler";

import * as qrCodeController from '@/src/presentation/controllers/qr-code.controller.js';

const router = Router();

// Public endpoints - no auth required
router.get('/qr-code/public-code/:publicCode', asyncHandler(qrCodeController.getByPublicCodeAsync));
router.get('/qr-codes/:publicCode/image', asyncHandler(qrCodeController.generateQrImageAsync));

// Protected endpoints - auth required
router.post('/qr-codes', asyncHandler(qrCodeController.createAsync));
router.get('/qr-codes', asyncHandler(qrCodeController.getAllAsync));
router.get('/qr-codes/:id', asyncHandler(qrCodeController.getByIdAsync));
router.put('/qr-codes/:id/item', asyncHandler(qrCodeController.updateItemAsync));
router.get('/qr-codes/:id/image', asyncHandler(qrCodeController.generateQrImageAsync));

export default router;
