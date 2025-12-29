import { Model, ClientSession, UpdateQuery } from "mongoose";

type SoftDeletable = { deletedAt?: Date | null };
type HasId<TId> = { _id: TId };
type MongoFilter<T> = Partial<T & { _id: unknown; deletedAt: Date | null }>;

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
        const filter = { _id: normalized, deletedAt: null } as MongoFilter<T>;
        const q = model.findOne(filter).lean<T>();
        if (session) q.session(session);
        return (await q.exec()) as T | null;
    };

    const findOne = async (filter: MongoFilter<T>, session?: ClientSession): Promise<T | null> => {
        const combinedFilter: MongoFilter<T> = { ...filter, deletedAt: null };
        const q = model.findOne(combinedFilter).lean<T>();
        if (session) q.session(session);
        return (await q.exec()) as T | null;
    };

    const update = async (
        id: string | TId,
        updateData: UpdateQuery<T>,
        session?: ClientSession
    ): Promise<T | null> => {
        const normalized = normalizeId(id);
        const filter = { _id: normalized, deletedAt: null } as MongoFilter<T>;
        const q = model
            .findOneAndUpdate(filter, updateData, {
                new: true,
                runValidators: true,
            })
            .lean<T>();

        if (session) q.session(session);
        return (await q.exec()) as T | null;
    };

    const exists = async (filter: MongoFilter<T>, session?: ClientSession): Promise<boolean> => {
        const combinedFilter: MongoFilter<T> = { ...filter, deletedAt: null };
        const q = model.exists(combinedFilter);
        if (session) q.session(session);
        return !!(await q);
    };

    return { create, findById, findOne, update, exists };
};  