import { Router } from "express";
import asyncHandler from "express-async-handler";
import * as orderController from "@/src/presentation/controllers/order.controller.js";

const router = Router();

// Public endpoint - no auth required (for PayOS webhook)
router.post('/payment-webhook', asyncHandler(orderController.handlePaymentWebhookAsync));

// Protected endpoint - auth required
router.post('/link-payment', asyncHandler(orderController.createLinkPaymentAsync));

export default router;