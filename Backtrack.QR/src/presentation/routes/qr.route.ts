import { Router } from 'express';
import asyncHandler from "express-async-handler";

import * as QrController from '@/src/presentation/controllers/qr.controller.js';

const router = Router();

router.get('/me', asyncHandler(QrController.getQrByUserIdAsync));
export default router;