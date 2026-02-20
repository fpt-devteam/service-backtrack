import { Request } from 'express';
export const HeaderNames = {
  CorrelationId: 'X-Correlation-Id',
  AuthId: 'X-Auth-Id',
  AuthEmail: 'X-Auth-Email',
  AuthName: 'X-Auth-Name',
  AuthAvatarUrl: 'X-Auth-Avatar-Url',
} as const;

export const getHeader = (req: Request, key: string): string | undefined => {
  return req.headers[key.toLowerCase()] as string | undefined;
};