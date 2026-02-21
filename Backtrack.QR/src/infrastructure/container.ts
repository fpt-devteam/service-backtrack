import { createUserRepository } from '@/src/infrastructure/database/repositories/user.repository.js';
import { getQrByUserIdUseCase } from '@/src/application/usecases/qr/get-qr-by-user-id.usecase.js';
import { createQrRepository } from '@/src/infrastructure/database/repositories/qr.repository.js';

// Layer 1: repositories
const userRepository = createUserRepository();
const qrRepository = createQrRepository();

// Layer 2: use cases (inject repositories)
// QR use cases
export const getQrByUserId = getQrByUserIdUseCase({ qrRepository });