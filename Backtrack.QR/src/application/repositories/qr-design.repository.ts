import { QrDesign } from '@/src/domain/entities/qr-design.entity.js';

export type UpdateQrDesignFields = Partial<Pick<QrDesign, 'size' | 'color' | 'backgroundColor' | 'quietZone' | 'ecl'>> & {
  logo?: Partial<QrDesign['logo']>;
  gradient?: {
    enabled?: boolean;
    colors?: [string, string];
    direction?: [number, number, number, number];
  };
};

export type QrDesignRepository = {
  findByUserId: (userId: string) => Promise<QrDesign | null>;
  createDefaultForUser: (userId: string) => Promise<QrDesign>;
  updateByUserId: (userId: string, fields: UpdateQrDesignFields) => Promise<QrDesign | null>;
};
