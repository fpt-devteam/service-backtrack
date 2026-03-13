import { z } from 'zod'

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const EmailAddressSchema = z.union([
  z.string().regex(emailRegex, 'Invalid email address'),
  z.array(z.string().regex(emailRegex, 'Invalid email address')),
])

export const EmailSendRequestSchema = z.object({
  to: EmailAddressSchema,
  subject: z.string().min(1, 'Subject is required'),
  text: z.string().min(1, 'Text is required'),
  html: z.string().min(1, 'HTML is required'),
  cc: EmailAddressSchema.optional(),
  bcc: EmailAddressSchema.optional(),
})

export type EmailSendRequest = z.infer<typeof EmailSendRequestSchema>