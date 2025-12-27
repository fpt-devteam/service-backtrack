import mongoose, { HydratedDocument, Schema } from "mongoose";

export interface IItem {
    name: string;
    description: string;
    imageUrls: string[];
}
export interface IQrCode {
    publicCode: string;
    ownerId: string;
    item?: IItem | null;
    linkedAt?: Date | null;
    createdAt: Date;
    updatedAt: Date;
    deletedAt?: Date | null;
}

export type QrCodeDocument = HydratedDocument<IQrCode>;

const ItemSchema = new Schema<IItem>(
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

export const QrCode = mongoose.model<IQrCode>('QrCode', QrCodeSchema);

