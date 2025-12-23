import { Router } from 'express';
import * as ItemController from '@/src/controllers/item.controller.js';

const router = Router();

router.get('/', ItemController.getAllAsync);

router.get('/:id', ItemController.getByIdAsync);

router.post('/', ItemController.createAsync);
export default router;
