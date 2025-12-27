import mongoose, { Schema } from 'mongoose';

const UserSchema = new Schema({
  _id: { type: String, required: true },
  email: { type: String, required: true, index: true },
  displayName: { type: String, default: null },
  avatarUrl: { type: String, default: null },
  createdAt: { type: Date, required: true },
  updatedAt: { type: Date, default: null },
  deletedAt: { type: Date, default: null },
  syncedAt: { type: Date, default: Date.now },
});

export const User = mongoose.model('User', UserSchema);
