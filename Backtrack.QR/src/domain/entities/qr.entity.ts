export const DEFAULT_QR_NOTE = "Hi there! Thank you so much for scanning this QR code. If you've found my item, I would really appreciate it if you could help return it to me. I'll gladly offer a reward as a token of my appreciation!";

export type Qr = {
  id: string;
  userId: string;
  publicCode: string;
  note: string;
  createdAt: Date;
  updatedAt: Date;
}