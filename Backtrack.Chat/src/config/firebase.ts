import * as admin from 'firebase-admin';
import { env } from '@/config/environment';
import logger from '@/utils/logger';

export function initializeFirebase(): void {
  if (admin.apps.length > 0) return;

  if (!env.FIREBASE_SERVICE_ACCOUNT_JSON_BASE64) {
    if (env.NODE_ENV === 'production') {
      throw new Error('FIREBASE_SERVICE_ACCOUNT_JSON_BASE64 is required in production');
    }
    logger.warn('FIREBASE_SERVICE_ACCOUNT_JSON_BASE64 not set — WebSocket JWT verification disabled (dev mode)');
    return;
  }

  const serviceAccountJson = Buffer.from(env.FIREBASE_SERVICE_ACCOUNT_JSON_BASE64, 'base64').toString('utf-8');
  const serviceAccount = JSON.parse(serviceAccountJson);

  admin.initializeApp({
    credential: admin.credential.cert(serviceAccount),
  });

  logger.info('Firebase Admin initialized for WebSocket JWT self-verification');
}

export function getFirebaseAuth(): admin.auth.Auth | null {
  if (admin.apps.length === 0) return null;
  return admin.auth();
}
