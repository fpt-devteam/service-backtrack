import { Router } from "express";
import asyncHandler from "express-async-handler";
import * as packageController from "@/src/presentation/controllers/package.controller.js";

const router = Router();

router.get('/packages', asyncHandler(packageController.getAllAsync));
router.get('/packages/:id', asyncHandler(packageController.getByIdAsync));
router.post('/packages', asyncHandler(packageController.createAsync));
router.put('/packages/:id', asyncHandler(packageController.updateAsync));
router.delete('/packages/:id', asyncHandler(packageController.deleteAsync));

export default router;
