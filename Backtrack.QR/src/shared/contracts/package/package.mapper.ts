import { IPackage } from "@/src/infrastructure/database/models/package.model.js";
import { PackageResponse } from "./package.response.js";
import { toVietnamISOStringOrDefault } from "@/src/shared/utils/timezone.js";

export const toPackageResponse = (pkg: IPackage): PackageResponse => {
    return {
        id: pkg._id.toString(),
        name: pkg.name,
        price: pkg.price,
        qrCount: pkg.qrCount,
        description: pkg.description,
        isActive: pkg.isActive,
        createdAt: toVietnamISOStringOrDefault(pkg.createdAt),
        deletedAt: pkg.deleteAt ? toVietnamISOStringOrDefault(pkg.deleteAt) : null,
    };
};