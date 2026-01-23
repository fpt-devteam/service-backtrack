import { NotificationChannel, NotificationEvent, PushProviders, NotificationStatus } from '../types/notification.type'
import mongoose, { Schema } from 'mongoose'

const NotificationSchema = new Schema(
  {
    userId: { type: String, index: true, required: true },
    title: { type: String, default: null },
    body: { type: String, default: null },
    data: { type: Schema.Types.Mixed, default: null },
    isArchived: { type: Boolean, default: false },
    isRead: { type: Boolean, default: false },
    sentAt: { type: Date, default: new Date(0) },

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

    status: {
      type: String,
      enum: Object.values(NotificationStatus),
      default: NotificationStatus.Pending,
    },

    source: {
      name: { type: String, required: true, trim: true },
      eventId: { type: String, required: true, trim: true },
    },

    provider: {
      type: String,
      enum: Object.values(PushProviders),
      required: true,
    },
  },
  { timestamps: true },
)

NotificationSchema.index({ userId: 1, createdAt: -1 })
NotificationSchema.index({ userId: 1, isRead: 1, createdAt: -1 })
NotificationSchema.index({ source: 1, sourceEventId: 1 }, { unique: true, sparse: true })

export const Notification = mongoose.model('Notification', NotificationSchema)
