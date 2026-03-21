import { QrDesign } from '@/src/domain/entities/qr-design.entity.js';

type QrDesignDocument = {
  _id: { toString: () => string };
  userId: string;
  size: number;
  color: string;
  backgroundColor: string;
  quietZone: number;
  ecl: QrDesign['ecl'] | string;
  logo?: {
    url: string;
    size: number;
    margin: number;
    borderRadius: number;
    backgroundColor: string;
  } | null;
  gradient?: {
    enabled: boolean;
    colors: [string, string] | string[];
    direction: [number, number, number, number] | number[];
  } | null;
  createdAt: Date;
  updatedAt: Date;
};

export const qrDesignToDomain = (doc: QrDesignDocument): QrDesign => ({
  id: doc._id.toString(),
  userId: doc.userId,
  size: doc.size,
  color: doc.color,
  backgroundColor: doc.backgroundColor,
  quietZone: doc.quietZone,
  ecl: (['L', 'M', 'Q', 'H'].includes(doc.ecl) ? doc.ecl : 'H') as QrDesign['ecl'],
  logo: {
    url: doc.logo?.url ?? '',
    size: doc.logo?.size ?? 50,
    margin: doc.logo?.margin ?? 2,
    borderRadius: doc.logo?.borderRadius ?? 0,
    backgroundColor: doc.logo?.backgroundColor ?? 'transparent',
  },
  gradient: {
    enabled: Boolean(doc.gradient?.enabled),
    colors: [doc.gradient?.colors?.[0] ?? '#000000', doc.gradient?.colors?.[1] ?? '#000000'],
    direction: [
      doc.gradient?.direction?.[0] ?? 0,
      doc.gradient?.direction?.[1] ?? 0,
      doc.gradient?.direction?.[2] ?? 1,
      doc.gradient?.direction?.[3] ?? 1,
    ],
  },
  createdAt: doc.createdAt,
  updatedAt: doc.updatedAt,
});
