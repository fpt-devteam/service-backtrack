import { UserRepository } from '@/src/application/repositories/user.repository.js';
import { User } from '@/src/domain/entities/user.entity.js';
import { Result, success, failure } from '@/src/shared/core/result.js';
import { UserErrors } from '@/src/application/errors/user.error.js';

type Deps = { userRepository: UserRepository };

export const getUserByIdUseCase = (deps: Deps) => async (id: string): Promise<Result<User>> => {
  const user = await deps.userRepository.findById(id);
  if (!user) return failure(UserErrors.NotFound);
  return success(user);
};