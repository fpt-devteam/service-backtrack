import express from 'express'
import NotificationController from '../controllers/notification.controller'
import { validateBody, validateQuery } from '@src/middlewares/validation.middleware'
import { NotificationSendPushRequestSchema, ArchivedStatusUpdateAllRequestSchema, ArchivedStatusUpdateRequestSchema, NotificationOptionsSchema, ReadStatusUpdateAllRequestSchema, ReadStatusUpdateRequestSchema } from '@src/contracts/requests/notification.request'

const router = express.Router()

router.get('/', validateQuery(NotificationOptionsSchema), NotificationController.getAsync.bind(NotificationController))
router.post('/', validateBody(NotificationSendPushRequestSchema), NotificationController.createAsync.bind(NotificationController))
router.put('/read', validateBody(ReadStatusUpdateRequestSchema), NotificationController.updateReadStatusAsync.bind(NotificationController))
router.put('/archive', validateBody(ArchivedStatusUpdateRequestSchema), NotificationController.updateArchivedStatusAsync.bind(NotificationController))
router.put('/read-all', validateBody(ReadStatusUpdateAllRequestSchema), NotificationController.updateAllReadStatusAsync.bind(NotificationController))
router.put('/archive-all', validateBody(ArchivedStatusUpdateAllRequestSchema), NotificationController.updateAllArchivedStatusAsync.bind(NotificationController))

export default router
