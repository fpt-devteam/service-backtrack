import { ApiResponse } from '@src/types/shared.type'
import { DevicePlatformType } from '@src/types/device.type'

export type DeviceItem = {
  _id: string
  userId: string
  token: string
  platform: DevicePlatformType
  deviceId: string
  appVersion: string | null
  isActive: boolean
  lastSeenAt: Date
  createdAt: Date
  updatedAt: Date
}

export type DeviceRegisterResult = {
  deviceId: string
  isActive: boolean
  lastSeenAt: Date
}

export type DeviceUnregisterResult = {
  deviceId: string
  isActive: boolean
}

export type DeviceRegisterResponse = ApiResponse<DeviceRegisterResult>

export type DeviceUnregisterResponse = ApiResponse<DeviceUnregisterResult>
