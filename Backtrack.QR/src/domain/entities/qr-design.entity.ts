export const QR_ECL = {
  L: 'L',
  M: 'M',
  Q: 'Q',
  H: 'H',
} as const;

export type QrEclType = (typeof QR_ECL)[keyof typeof QR_ECL];

export type QrDesign = {
  id: string;
  userId: string;
  size: number;
  color: string;
  backgroundColor: string;
  quietZone: number;
  ecl: QrEclType;
  logo: {
    url: string;
    size: number;
    margin: number;
    borderRadius: number;
    backgroundColor: string;
  };
  gradient: {
    enabled: boolean;
    colors: [string, string];
    direction: [number, number, number, number];
  };
  createdAt: Date;
  updatedAt: Date;
};
