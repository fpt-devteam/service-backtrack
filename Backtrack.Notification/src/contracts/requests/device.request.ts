import { z } from 'zod'

// Register device schema
export const RegisterDeviceBodySchema = z.object({
  token: z.string().min(1, 'Token is required').trim(),
  platform: z.enum(['ios', 'android']),
  deviceId: z.string().min(1, 'DeviceId is required').trim(),
  appVersion: z.string().trim().optional(),
})

export type RegisterDeviceBody = z.infer<typeof RegisterDeviceBodySchema>

// Unregister device schema
export const UnregisterDeviceBodySchema = z.object({
  token: z.string().min(1, 'Token is required').trim(),
  deviceId: z.string().trim().optional(),
})

export type UnregisterDeviceBody = z.infer<typeof UnregisterDeviceBodySchema>
