import { DevicePlatform } from '@src/types/device.type'
import mongoose, { Schema } from 'mongoose'

const DeviceSchema = new Schema(
  {
    userId: {
      type: String,
      index: true,
      required: true,
    },

    token: {
      type: String,
      required: true,
      index: true,
    },

    platform: {
      type: String,
      enum: Object.values(DevicePlatform),
      required: true,
    },

    deviceId: {
      type: String,
      required: true,
    },

    isActive: {
      type: Boolean,
      default: true,
    },

    lastSeenAt: {
      type: Date,
      default: Date.now,
    },
  },
  { timestamps: true },
)

DeviceSchema.index({ userId: 1, deviceId: 1 }, { unique: true })
DeviceSchema.index({ userId: 1, isActive: 1 })
DeviceSchema.index({ token: 1 })

export const Device = mongoose.model('Device', DeviceSchema)
