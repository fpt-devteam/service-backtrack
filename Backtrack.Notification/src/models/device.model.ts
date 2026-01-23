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

    appVersion: {
      type: String,
      default: null,
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
  {
    timestamps: true,
  },
)

// Enforce uniqueness on (userId, deviceId)
DeviceSchema.index({ userId: 1, deviceId: 1 }, { unique: true })

// Index for active device queries
DeviceSchema.index({ userId: 1, isActive: 1 })

// Index for token lookups
DeviceSchema.index({ token: 1 })

export const Device = mongoose.model('Device', DeviceSchema)
