import mongoose from "mongoose";
import { IPackage, PackageModel } from "../database/models/package.model.js";
import { createBaseRepo } from "./common/base.repository.js";

const baseRepo = createBaseRepo<IPackage, mongoose.Types.ObjectId>(PackageModel, (id) => new mongoose.Types.ObjectId(id));
const findAll = async (): Promise<IPackage[]> => {
    return await PackageModel.find({}).lean().exec();
};
const deleteById = async (id: string): Promise<IPackage | null> => {
    const normalizedId = new mongoose.Types.ObjectId(id);
    const deletedPackage = await PackageModel.findOneAndUpdate(
        { _id: normalizedId, deleteAt: null },
        { deleteAt: new Date(), isActive: false },
        { new: true }
    ).lean().exec();
    return deletedPackage as IPackage | null;
}
const updateById = async (
    id: string,
    updateData: Partial<IPackage>
): Promise<IPackage | null> => {
    const normalizedId = new mongoose.Types.ObjectId(id);
    const updatedPackage = await PackageModel.findOneAndUpdate(
        { _id: normalizedId, deleteAt: null },
        updateData,
        { new: true, runValidators: true }
    ).lean().exec();
    return updatedPackage as IPackage | null;
};

const existsByName = async (name: string, excludeId?: string): Promise<boolean> => {
    const query: any = { name, deleteAt: null };
    if (excludeId) {
        query._id = { $ne: new mongoose.Types.ObjectId(excludeId) };
    }
    const count = await PackageModel.countDocuments(query).exec();
    return count > 0;
};

export const packageRepository = {
    ...baseRepo,
    findAll,
    deleteById,
    updateById,
    existsByName,
};