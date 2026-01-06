import crypto from 'crypto';

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