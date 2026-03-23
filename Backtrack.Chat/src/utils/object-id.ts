import mongoose from 'mongoose';

/**
 * Safely converts a string or ObjectId to a MongoDB ObjectId
 * Returns null if the input is not a valid ObjectId
 *
 * @param id - The string or ObjectId to convert
 * @returns A valid ObjectId or null
 */
export const toObjectIdOrNull = (
    id: string | mongoose.Types.ObjectId
): mongoose.Types.ObjectId | null => {
    if (id instanceof mongoose.Types.ObjectId) return id;
    if (!mongoose.Types.ObjectId.isValid(id)) return null;
    return new mongoose.Types.ObjectId(id);
};

export const toStringOrNull = (
    id: string | mongoose.Types.ObjectId
): string | null => {
    if (typeof id === 'string') return id;
    if (id instanceof mongoose.Types.ObjectId) return id.toString();
    return null;
};
/**
 * Checks if a value is a valid ObjectId
 *
 * @param id - The value to check
 * @returns True if the value is a valid ObjectId, false otherwise
 */
export const isValidObjectId = (
    id: string | mongoose.Types.ObjectId | null | undefined
): boolean => {
    if (!id) return false;
    if (id instanceof mongoose.Types.ObjectId) return true;
    return mongoose.Types.ObjectId.isValid(id);
};

/**
 * Converts a Mongoose document interface into the shape returned by `.lean()`.
 *
 * Mongoose adds an `id` virtual (string) to every document, but `.lean()` strips
 * virtuals — leaving only the raw `_id: Types.ObjectId`.
 * This utility removes the `id` field and adds the correct `_id` type so callers
 * don't need to repeat `Omit<T, 'id'> & { _id: Types.ObjectId }` manually.
 *
 * @example
 * type LeanDirect  = ToLeanDoc<IDirectConversation>;
 * type LeanSupport = ToLeanDoc<ISupportConversation>;
 *
 * const doc = await DirectConversation.findById(id).lean().exec() as LeanDirect;
 * doc._id.toString(); // ✅ Types.ObjectId — no `any` cast needed
 */
export type ToLeanDoc<T extends { id?: unknown }> =
    Omit<T, 'id'> & { _id: mongoose.Types.ObjectId };
