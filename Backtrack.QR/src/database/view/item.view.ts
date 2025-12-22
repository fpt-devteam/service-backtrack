import { Item } from "@/src/database/models/item.model.js";
import { QrCode } from "@/src/database/models/qr-code.models.js";

export type ItemWithQrResult = {
    item: InstanceType<typeof Item>;
    qrCode: InstanceType<typeof QrCode>;
};