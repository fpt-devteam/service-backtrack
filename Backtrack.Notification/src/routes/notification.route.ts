import express from 'express'
import NotificationController from '../controllers/notification.controller'
import {
  validateBody,
  validateQuery,
} from '@src/middlewares/validation.middleware'
import {
  NotificationOptionsSchema,
  NotificationSendPushRequestSchema,
  NotificationStatusUpdateRequestSchema,
} from '@src/contracts/requests/notification.request'

const router = express.Router()

router.post(
  '/',
  validateBody(NotificationSendPushRequestSchema),
  NotificationController.createAsync.bind(NotificationController),
)

router.get(
  '/',
  validateQuery(NotificationOptionsSchema),
  NotificationController.getAsync.bind(NotificationController),
)

router.get(
  '/unread-count',
  NotificationController.getUnreadCountAsync.bind(NotificationController),
)

router.put(
  '/',
  validateBody(NotificationStatusUpdateRequestSchema),
  NotificationController.updateStatusNotificationsAsync.bind(
    NotificationController,
  ),
)

export default router
