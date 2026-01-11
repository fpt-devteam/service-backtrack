import HTTP_STATUS_CODES from '../constants/HTTP_STATUS_CODES';
import { AppError } from './app-error';

export const ErrorCodes = {
  // Missing Required Fields
  MissingNotificationId: new AppError(
    'MissingNotificationId',
    'Notification ID is required',
    HTTP_STATUS_CODES.BadRequest,

  ),
};