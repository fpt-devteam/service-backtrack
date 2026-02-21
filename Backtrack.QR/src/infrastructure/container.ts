import { createUserRepository } from '@/src/infrastructure/database/repositories/user.repository.js';
import { getUserByIdUseCase } from '@/src/application/usecases/users/get-user-by-id.usecase.js';
import { createUserUseCase } from '@/src/application/usecases/users/create-user.usecase.js';
import { getQrByUserIdUseCase } from '@/src/application/usecases/qr/get-qr-by-user-id.usecase.js';
import { createQrRepository } from '@/src/infrastructure/database/repositories/qr.repository.js';

// Layer 1: repositories
const userRepository = createUserRepository();
const qrRepository = createQrRepository();

// Layer 2: use cases (inject repositories)
// User use cases
export const getUserById = getUserByIdUseCase({ userRepository });
export const createUser = createUserUseCase({ userRepository });

// QR use cases
export const getQrByUserId = getQrByUserIdUseCase({ qrRepository });