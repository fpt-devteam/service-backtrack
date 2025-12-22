import mongoose, { Schema } from "mongoose";

const ItemSchema = new Schema({
    name: { type: String, required: true },
    description: { type: String, required: true },
    imageUrls: { type: [String], required: true },
    ownerId: { type: String, required: true, index: true },
    createAt: { type: Date, default: Date.now },
    updateAt: { type: Date, default: null },
    deletedAt: { type: Date, default: null },
});

export const Item = mongoose.model('Item', ItemSchema);