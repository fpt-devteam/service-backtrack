import { IUser, UserModel } from '@/src/infrastructure/database/models/user.model.js';
import { createBaseRepo } from './common/base.repository.js';

const userBaseRepo = createBaseRepo<IUser, string>(UserModel, (id) => id);
export const userRepository = {
    ...userBaseRepo
};
