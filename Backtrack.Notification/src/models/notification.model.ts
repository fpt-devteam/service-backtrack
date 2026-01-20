import {
  NotificationChannel,
  NotificationEvent,
  NotificationStatus,
} from '../types/notification.type'
import mongoose, { Schema } from 'mongoose'

const NotificationSchema = new Schema(
  {
    userId: {
      type: String,
      index: true,
      required: true,
    },

    channel: {
      type: String,
      enum: Object.values(NotificationChannel),
      required: true,
    },

    type: {
      type: String,
      enum: Object.values(NotificationEvent),
      required: true,
    },

    title: {
      type: String,
      default: null,
    },

    body: {
      type: String,
      default: null,
    },

    data: {
      type: Schema.Types.Mixed,
      default: null,
    },

    status: {
      type: String,
      enum: Object.values(NotificationStatus),
      default: NotificationStatus.Pending,
    },

    sentAt: { type: Date, default: new Date(0) },
    isRead: { type: Boolean, default: false },
  },
  {
    timestamps: true,
  },
)

export const Notification = mongoose.model('Notification', NotificationSchema)
