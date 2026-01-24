import { z } from 'zod'

export const SyncDeviceRequestSchema = z.object({
  token: z.string().min(1, 'Token is required').trim(),
  deviceId: z.string().min(1, 'DeviceId is required').trim(),
})
export type UpdatePushTokenRequest = z.infer<typeof SyncDeviceRequestSchema>
