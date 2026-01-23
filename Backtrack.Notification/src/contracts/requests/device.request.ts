import { DevicePlatform } from '@src/types/device.type'
import { z } from 'zod'

export const RegisterDeviceBodySchema = z.object({
  token: z.string().min(1, 'Token is required').trim(),
  platform: z.enum(Object.values(DevicePlatform)),
  deviceId: z.string().min(1, 'DeviceId is required').trim(),
})

export type RegisterDeviceBody = z.infer<typeof RegisterDeviceBodySchema>

export const UnregisterDeviceBodySchema = z.object({
  token: z.string().min(1, 'Token is required').trim(),
  deviceId: z.string().trim().optional(),
})

export type UnregisterDeviceBody = z.infer<typeof UnregisterDeviceBodySchema>
