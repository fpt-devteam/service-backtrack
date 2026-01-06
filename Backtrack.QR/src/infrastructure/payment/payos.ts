import { PayOS } from '@payos/node';
import { env } from '@/src/shared/configs/env.js';

export const payosClient = new PayOS({
  clientId: env.PAYOS_CLIENT_ID,
  apiKey: env.PAYOS_API_KEY,
  checksumKey: env.PAYOS_CHECKSUM_KEY
});