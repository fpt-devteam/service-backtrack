import mongoose from "mongoose";
import { IPackage, PackageModel } from "../database/models/package.model.js";
import { createBaseRepo } from "./common/base.repository.js";

const baseRepo = createBaseRepo<IPackage, mongoose.Types.ObjectId>(PackageModel, (id) => new mongoose.Types.ObjectId(id));
export const packageRepository = {    
    ...baseRepo,
};