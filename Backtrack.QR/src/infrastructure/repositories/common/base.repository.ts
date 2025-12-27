import { Model, ClientSession, UpdateQuery } from "mongoose";

type SoftDeletable = { deletedAt?: Date | null };
type HasId<TId> = { _id: TId };
type RepoFilter<T> = Record<string, any>;

export const createBaseRepo = <T extends SoftDeletable & HasId<TId>, TId>(
    model: Model<T>,
    normalizeId: (id: string | TId) => TId
) => {
    const create = async (data: Partial<T>, session?: ClientSession): Promise<T> => {
        const doc = new model(data);
        const saved = await doc.save({ session });
        return saved.toObject() as T;
    };

    const findById = async (id: string | TId, session?: ClientSession): Promise<T | null> => {
        const normalized = normalizeId(id);
        const q = model.findOne({ _id: normalized, deletedAt: null } as any).lean<T>();
        if (session) q.session(session);
        return (await q.exec()) as T | null;
    };

    const findOne = async (filter: RepoFilter<T>, session?: ClientSession): Promise<T | null> => {
        const q = model.findOne({ ...filter, deletedAt: null } as any).lean<T>();
        if (session) q.session(session);
        return (await q.exec()) as T | null;
    };

    const update = async (
        id: string | TId,
        updateData: UpdateQuery<T>,
        session?: ClientSession
    ): Promise<T | null> => {
        const normalized = normalizeId(id);
        const q = model
            .findOneAndUpdate({ _id: normalized, deletedAt: null } as any, updateData, {
                new: true,
                runValidators: true,
            })
            .lean<T>();

        if (session) q.session(session);
        return (await q.exec()) as T | null;
    };

    const exists = async (filter: RepoFilter<T>, session?: ClientSession): Promise<boolean> => {
        const q = model.exists({ ...filter, deletedAt: null } as any);
        if (session) q.session(session);
        return !!(await q);
    };

    return { create, findById, findOne, update, exists };
};  