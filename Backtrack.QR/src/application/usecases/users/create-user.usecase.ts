import { UserRepository } from '@/src/application/repositories/user.repository.js';
import { User } from '@/src/domain/entities/user.entity.js';
import { failure, Result, success } from '@/src/shared/core/result.js';
import { UserErrors } from '@/src/application/errors/user.error.js';
import { randomUUID } from 'node:crypto';

type Deps = { userRepository: UserRepository };

export const createUserUseCase = (deps: Deps) => async (input: Omit<User, 'id' | 'createdAt' | 'updatedAt'>): Promise<Result<User>> => {
  const existing = input.email ? await deps.userRepository.findByEmail(input.email) : null;
  if (existing) return failure(UserErrors.EmailExists);
  const savedUser = await deps.userRepository.save({
    id: randomUUID(),
    ...input,
  }
  );
  return success(savedUser);
};