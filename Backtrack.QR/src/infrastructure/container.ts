import { createUserRepository } from '@/src/infrastructure/database/repositories/user.repository.js';
import { getUserByIdUseCase } from '@/src/application/usecases/users/get-user-by-id.usecase.js';
import { createUserUseCase } from '@/src/application/usecases/users/create-user.usecase.js';

// Layer 1: repositories
const userRepository = createUserRepository();

// Layer 2: use cases (inject repositories)
export const getUserById = getUserByIdUseCase({ userRepository });
export const createUser = createUserUseCase({ userRepository });