import { IUser, User } from '@/src/database/models/user.model.js';
import { createBaseRepo } from './common/base.repository.js';

const userBaseRepo = createBaseRepo<IUser, string>(User, (id) => id);
export const userRepository = {
    ...userBaseRepo
};
