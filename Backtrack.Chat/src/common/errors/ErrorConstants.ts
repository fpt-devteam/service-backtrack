import { AppError } from './AppError';
import HTTP_STATUS_CODES from '../constants/HTTP_STATUS_CODES';

export const ErrorCodes = {
  InvalidPagedQuery: new AppError(
    'InvalidPagedQuery',
    'Limit and offset must be greater than 0',
    HTTP_STATUS_CODES.BadRequest,
  ),  

  UserNotFound: new AppError(
    'UserNotFound',
    'User not found',
    HTTP_STATUS_CODES.NotFound,
  ),

  ConversationNotFound: new AppError(
    'ConversationNotFound',
    'Conversation not found',
    HTTP_STATUS_CODES.NotFound,
  ),
  ConflictError: new AppError(
    'ConflictError',
    'Resource conflict occurred',
    HTTP_STATUS_CODES.Conflict,
  ),  
  InternalServerError: new AppError(
    'InternalServerError',
    'An unexpected error occurred',
    HTTP_STATUS_CODES.InternalServerError,
  ),
};