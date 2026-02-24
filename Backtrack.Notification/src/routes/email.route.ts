import express from 'express'
import EmailController from '@src/controllers/email.controller'
import { validateBody } from '@src/middlewares/validation.middleware'
import {
  EmailSendRequestSchema,
} from '@src/contracts/requests/email.request'

const router = express.Router()

router.post(
  '/',
  validateBody(EmailSendRequestSchema),
  EmailController.sendEmailAsync.bind(EmailController),
)

router.get(
  '/verify',
  EmailController.verifyConnectionAsync.bind(EmailController),
)

export default router
