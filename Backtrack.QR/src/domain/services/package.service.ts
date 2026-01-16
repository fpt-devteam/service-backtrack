import { IPackage } from "@/src/infrastructure/database/models/package.model.js";
import { packageRepository } from "@/src/infrastructure/repositories/package.repository.js";
import { toPackageResponse } from "@/src/shared/contracts/package/package.mapper.js";
import { CreatePackageRequest, UpdatePackageRequest } from "@/src/shared/contracts/package/package.request.js";
import { PackageResponse } from "@/src/shared/contracts/package/package.response.js";
import { PackageErrors } from "@/src/shared/errors/catalog/package.error.js";
import { Result, success, failure } from "@/src/shared/utils/result.js";

export const createAsync = async (
    request: CreatePackageRequest
): Promise<Result<PackageResponse>> => {
    const nameExists = await packageRepository.existsByName(request.name);
    if (nameExists) {
        return failure(PackageErrors.DuplicateName);
    }

    const packageData: Partial<IPackage> = {
        name: request.name,
        price: request.price,
        description: request.description,
        isActive: request.isActive,
        qrCount: request.qrCount,
    };
    const createdPackage = await packageRepository.create(packageData);
    if (!createdPackage) {
        return failure({
            kind: "Internal",
            code: "PackageCreationFailed",
            message: "Failed to create package.",
        });
    }
    return success(toPackageResponse(createdPackage));
};

export const findAllAsync = async (): Promise<Result<PackageResponse[]>> => {
    const packages = await packageRepository.findAll();
    const packageResponses = packages.map(toPackageResponse);
    return success(packageResponses);
}

export const findByIdAsync = async (id: string): Promise<Result<PackageResponse>> => {
    const pkg = await packageRepository.findById(id);
    if (!pkg) {
        return failure(PackageErrors.NotFound);
    }
    return success(toPackageResponse(pkg));
};

export const deleteByIdAsync = async (id: string): Promise<Result<void>> => {
    const deleted = await packageRepository.findById(id);
    if (!deleted) {
        return failure(PackageErrors.NotFound);
    }
    await packageRepository.deleteById(id);
    return success(undefined);
}

export const updateByIdAsync = async (
    id: string,
    request: UpdatePackageRequest
): Promise<Result<PackageResponse>> => {
    const pkg = await packageRepository.findById(id);
    if (!pkg) {
        return failure(PackageErrors.NotFound);
    }

    if (request.name) {
        const nameExists = await packageRepository.existsByName(request.name, id);
        if (nameExists) {
            return failure(PackageErrors.DuplicateName);
        }
    }

    const updatedPackage = await packageRepository.updateById(id, {
        name: request.name,
        price: request.price,
        qrCount: request.qrCount,
        description: request.description,
        isActive: request.isActive,
    });
    if (!updatedPackage) {
        return failure({
            kind: "Internal",
            code: "PackageUpdateFailed",
            message: "Failed to update package.",
        });
    }
    return success(toPackageResponse(updatedPackage));
};