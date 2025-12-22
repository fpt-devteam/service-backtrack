import { Router } from 'express';
import * as ItemController from '@/src/controllers/item.controller.js';

const router = Router();

router.get('/:id', ItemController.getById);

router.post('/', ItemController.create);

export default router;
