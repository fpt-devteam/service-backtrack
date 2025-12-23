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
