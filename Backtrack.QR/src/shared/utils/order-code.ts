import crypto from 'crypto';
import { env } from 'process';

export const generateOrderCode = (userId: string): number => {
  const timestamp = Date.now();
  
  const data = `${userId}-${timestamp}`;
  
  const hash = crypto
    .createHash('sha256')
    .update(data)
    .digest('hex');
  
  const orderCode = parseInt(hash.slice(0, 13), 16);  
  return orderCode;
}

export const generateOrderCodeSystem = (userId: string): string => {
  const timestamp = Date.now();
  
  const data = `${userId}-${timestamp}`;
  const hash = crypto
    .createHash('sha256')
    .update(data)
    .digest('hex');
  
  const orderCode = parseInt(hash.slice(0, 13), 16);  
  return `${env.PREFIX_ORDER_CODE}-${orderCode}`;
}