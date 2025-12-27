import mongoose, { Schema } from "mongoose";

export type Item = {
    name: string;
    description: string;
    imageUrls: string[];
};

export interface IQrCode {
    _id: mongoose.Types.ObjectId;
    publicCode: string;
    ownerId: string;
    item?: Item | null;
    linkedAt?: Date | null;
    createdAt?: Date;
    updatedAt?: Date;
    deletedAt?: Date | null;
}

const ItemSchema = new Schema<Item>(
    {
        name: { type: String, required: true },
        description: { type: String, required: true },
        imageUrls: { type: [String], required: true, default: [] },
    },
    { _id: false }
);

const QrCodeSchema = new Schema<IQrCode>(
    {
        publicCode: { type: String, required: true, index: true, unique: true },
        ownerId: {
            type: String,
            required: true,
            index: true,
        },
        item: {
            type: ItemSchema,
            required: false,
            default: null
        },
        linkedAt: { type: Date, default: null },
        deletedAt: { type: Date, default: null },
    },
    {
        timestamps: true,
    }
);

export const QrCodeModel = mongoose.model<IQrCode>('QrCode', QrCodeSchema);