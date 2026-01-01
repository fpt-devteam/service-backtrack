import { AppError } from './app-error';
import HTTP_STATUS_CODES from '../constants/HTTP_STATUS_CODES';

export const ErrorCodes = {


  // Missing Required Fields
  MissingUserId: new AppError(
    'MissingUserId',
    'User ID is required in X-Auth-Id header',
    HTTP_STATUS_CODES.Forbidden,
  ),
  MissingContent: new AppError(
    'MissingContent',
    'Content is required',
    HTTP_STATUS_CODES.BadRequest,
  ),
  MissingPartnerId: new AppError(
    'MissingPartnerId',
    'partnerId is required',
    HTTP_STATUS_CODES.BadRequest,
  ),
  MissingConversationId: new AppError(
    'MissingConversationId',
    'conversationId is required',
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

  NotParticipant: new AppError(
    'NotParticipant',
    'You are not a member of this conversation',
    HTTP_STATUS_CODES.NotFound,
  ),

  // Conflict Errors
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
  CannotCreateConversationWithYourself: new AppError(
    'CannotCreateConversationWithYourself',
    'Cannot create a conversation with yourself',
    HTTP_STATUS_CODES.BadRequest,
  ),
};
