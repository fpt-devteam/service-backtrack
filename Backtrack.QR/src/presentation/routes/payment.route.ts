import { Router } from "express";
import asyncHandler from "express-async-handler";
import * as paymentController from "@/src/presentation/controllers/payment.controller.js";
const router = Router();

router.get('/payment/failed', asyncHandler(paymentController.paymentFailedAsync));
router.get('/payment/succeed', asyncHandler(paymentController.paymentSucceedAsync));
router.get('/payment/re-create-link/:orderId', asyncHandler(paymentController.reCreateLinkPaymentAsync));
export default router;