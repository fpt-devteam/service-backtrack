import { Router } from "express";
import asyncHandler from "express-async-handler";
import * as orderController from "@/src/presentation/controllers/order.controller.js";

const router = Router();

// Public endpoint - no auth required (for PayOS webhook)
router.post('/order/payment-webhook', asyncHandler(orderController.handlePaymentWebhookAsync));

// Protected endpoint - auth required
router.post('/order/link-payment', asyncHandler(orderController.createLinkPaymentAsync));
router.get('/order-code/:code', asyncHandler(orderController.getOrderByCodeAsync));
router.get('/order/:id', asyncHandler(orderController.getOrderByIdAsync));
router.put('/order/update-status', asyncHandler(orderController.updateOrderStatusAsync));

export default router;