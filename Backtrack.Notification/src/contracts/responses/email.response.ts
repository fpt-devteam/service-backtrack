import { ApiResponse } from "@src/types/shared.type"

export type EmailSendResult = {
  sent: boolean
}

export type EmailSendResponse = ApiResponse<EmailSendResult>

export type EmailVerifyResult = {
  connected: boolean
}

export type EmailVerifyResponse = ApiResponse<EmailVerifyResult>;
