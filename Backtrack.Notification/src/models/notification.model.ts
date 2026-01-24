import { NOTIFICATION_EVENT, NOTIFICATION_STATUS } from '../types/notification.type'
import mongoose, { Schema } from 'mongoose'

const NotificationSchema = new Schema(
  {
    userId: { type: String, index: true, required: true },

    type: {
      type: String,
      enum: Object.values(NOTIFICATION_EVENT),
      required: true,
    },

    title: { type: String, required: true, trim: true },

    body: { type: String, required: true, trim: true },

    data: { type: Schema.Types.Mixed, default: null },

    status: {
      type: String,
      enum: Object.values(NOTIFICATION_STATUS),
      default: NOTIFICATION_STATUS.Unread,
    },

    sentAt: { type: Date, default: new Date(0) },

    source: {
      name: { type: String, required: true, trim: true },
      eventId: { type: String, required: true, trim: true },
    },
  },
  { timestamps: true },
)

NotificationSchema.index({ userId: 1, createdAt: -1 })
NotificationSchema.index({ userId: 1, isRead: 1, createdAt: -1 })
NotificationSchema.index({ source: 1, sourceEventId: 1 }, { unique: true, sparse: true })

export const Notification = mongoose.model('Notification', NotificationSchema)
