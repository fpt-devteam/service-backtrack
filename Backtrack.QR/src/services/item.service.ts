import * as ItemRepo from '@/src/repositories/item.repository.js';
import type { ItemDocument, CreateItemInput } from '@/src/repositories/item.repository.js';
import { Result, success, failure, isFailure } from '@/src/utils/result.js';
import { ItemErrors } from '@/src/errors/catalog/item.error.js';

const isValidItemName = (name: string): boolean => {
    return Boolean(name && name.trim().length > 0);
};

export const getByIdAsync = async (
    id: string
): Promise<Result<ItemDocument>> => {
    const result = await ItemRepo.getById(id);

    if (isFailure(result)) {
        return result;
    }

    if (result.value === null) {
        return failure(ItemErrors.NotFound);
    }

    return success(result.value);
};

export const createAsync = async (
    input: CreateItemInput
): Promise<Result<ItemDocument>> => {

    if (!isValidItemName(input.name)) {
        return failure(ItemErrors.InvalidName);
    }
    return await ItemRepo.create({
        ...input,
        name: input.name.trim()
    });
}

