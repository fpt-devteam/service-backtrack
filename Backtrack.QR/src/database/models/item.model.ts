import mongoose, { Schema } from "mongoose";

const ItemSchema = new Schema({
    name: { type: String, required: true },
    description: { type: String, required: false },
    imageUrls: { type: [String], required: false },
    userId: { type: String, required: true },
    createAt: { type: Date, default: Date.now },
    updateAt: { type: Date, default: null },
    deletedAt: { type: Date, default: null },
});

export const Item = mongoose.model('Item', ItemSchema);