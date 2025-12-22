import mongoose, { Schema } from "mongoose";

const QrCodeSchema = new Schema({
    publicCode: { type: String, required: true, index: true, unique: true },
    ownerId: { type: String, required: true, index: true },
    itemId: { type: String, required: false, index: true },
    status: { type: String, required: true, enum: ['linked', 'unlinked'], default: 'unlinked' },
    linkedAt: { type: Date, default: null },
    createAt: { type: Date, default: Date.now },
    updateAt: { type: Date, default: null },
    deletedAt: { type: Date, default: null },
});

export const QrCode = mongoose.model('QrCode', QrCodeSchema);

