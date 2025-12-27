import mongoose, { Model, ClientSession, UpdateQuery } from "mongoose";

export type WithId<T, TId = mongoose.Types.ObjectId> = T & { _id: TId };

type SoftDeletable = { deletedAt?: Date | null };
type RepoFilter<T> = Record<string, any>;

export const createBaseRepo = <T extends SoftDeletable, TId>(
    model: Model<T>,
    normalizeId: (id: string | TId) => TId
) => {
    const create = async (data: Partial<T>, session?: ClientSession) => {
        const doc = new model(data);
        const saved = await doc.save({ session });
        return saved.toObject() as WithId<T, TId>;
    };

    const findById = async (id: string | TId, session?: ClientSession) => {
        const normalized = normalizeId(id);
        const q = model.findOne({ _id: normalized, deletedAt: null } as any).lean();
        if (session) q.session(session);
        return (await q.exec()) as WithId<T, TId> | null;
    };

    const findOne = async (filter: RepoFilter<T>, session?: ClientSession) => {
        const q = model.findOne({ ...filter, deletedAt: null } as any).lean();
        if (session) q.session(session);
        return (await q.exec()) as WithId<T, TId> | null;
    };

    const update = async (id: string | TId, updateData: UpdateQuery<T>, session?: ClientSession) => {
        const normalized = normalizeId(id);
        const q = model
            .findOneAndUpdate({ _id: normalized, deletedAt: null } as any, updateData, {
                new: true,
                runValidators: true,
            })
            .lean();
        if (session) q.session(session);
        return (await q.exec()) as WithId<T, TId> | null;
    };

    const exists = async (filter: RepoFilter<T>, session?: ClientSession) => {
        const q = model.exists({ ...filter, deletedAt: null } as any);
        if (session) q.session(session);
        return !!(await q);
    };

    return { create, findById, findOne, update, exists };
};
