import express from 'express'
import NotificationController from '../controllers/notification.controller'

const router = express.Router()

router.get(
  '/',
  NotificationController.getNotifications.bind(NotificationController),
)

router.post(
  '/',
  NotificationController.sendNotification.bind(NotificationController),
)

router.patch(
  '/read-all',
  NotificationController.markAllAsRead.bind(NotificationController),
)

router.patch(
  '/read',
  NotificationController.markMultipleAsRead.bind(NotificationController),
)

export default router
