import { AppError } from './app-error';
import HTTP_STATUS_CODES from '../constants/HTTP_STATUS_CODES';

export const ErrorCodes = {
  // Validation Errors
  InvalidPagedQuery: new AppError(
    'InvalidPagedQuery',
    'Limit and offset must be greater than 0',
    HTTP_STATUS_CODES.BadRequest,
  ),
  EmptyMessageContent: new AppError(
    'EmptyMessageContent',
    'Message content cannot be empty',
    HTTP_STATUS_CODES.BadRequest,
  ),

  // Missing Required Fields
  MissingUserId: new AppError(
    'MissingUserId',
    'User ID is required in X-Auth-Id header',
    HTTP_STATUS_CODES.BadRequest,
  ),
  MissingContent: new AppError(
    'MissingContent',
    'Content is required',
    HTTP_STATUS_CODES.BadRequest,
  ),
  MissingPartnerInfo: new AppError(
    'MissingPartnerInfo',
    'partnerId is required',
    HTTP_STATUS_CODES.BadRequest,
  ),

  // Not Found Errors
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
  SenderNotFound: new AppError(
    'SenderNotFound',
    'Sender not found',
    HTTP_STATUS_CODES.NotFound,
  ),
  PartnerNotFound: new AppError(
    'PartnerNotFound',
    'Partner user not found',
    HTTP_STATUS_CODES.NotFound,
  ),

  // Forbidden Errors
  NotParticipant: new AppError(
    'NotParticipant',
    'You are not a member of this conversation',
    HTTP_STATUS_CODES.Forbidden,
  ),

  // Conflict Errors
  ConversationAlreadyExists: new AppError(
    'ConversationAlreadyExists',
    'Conversation already exists between these users',
    HTTP_STATUS_CODES.Conflict,
  ),
  ConflictError: new AppError(
    'ConflictError',
    'Resource conflict occurred',
    HTTP_STATUS_CODES.Conflict,
  ),

  // Server Errors
  InternalServerError: new AppError(
    'InternalServerError',
    'An unexpected error occurred',
    HTTP_STATUS_CODES.InternalServerError,
  ),
};