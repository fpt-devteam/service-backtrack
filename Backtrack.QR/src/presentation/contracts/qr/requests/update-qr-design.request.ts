import { QR_ECL } from '@/src/domain/entities/qr-design.entity.js';
import { z } from 'zod';

const SIZE_MIN = 100;
const SIZE_MAX = 2000;
const QUIET_ZONE_MIN = 0;
const QUIET_ZONE_MAX = 100;
const LOGO_SIZE_MIN = 0;
const LOGO_SIZE_MAX = 1000;
const LOGO_MARGIN_MIN = 0;
const LOGO_MARGIN_MAX = 100;
const LOGO_BORDER_RADIUS_MIN = 0;
const LOGO_BORDER_RADIUS_MAX = 500;
const LOGO_BG_COLOR_MIN_LENGTH = 1;
const LOGO_BG_COLOR_MAX_LENGTH = 64;
const GRADIENT_DIRECTION_MIN = -1;
const GRADIENT_DIRECTION_MAX = 1;

const colorSchema = z.string().trim().regex(/^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/, 'must be a valid hex color');

export const UpdateQrDesignRequestSchema = z.object({
  size: z.number().int().min(SIZE_MIN).max(SIZE_MAX).optional(),
  color: colorSchema.optional(),
  backgroundColor: colorSchema.optional(),
  quietZone: z.number().int().min(QUIET_ZONE_MIN).max(QUIET_ZONE_MAX).optional(),
  ecl: z.enum([QR_ECL.L, QR_ECL.M, QR_ECL.Q, QR_ECL.H]).optional(),
  logo: z.object({
    url: z.string().trim().max(2000).optional(),
    size: z.number().int().min(LOGO_SIZE_MIN).max(LOGO_SIZE_MAX).optional(),
    margin: z.number().int().min(LOGO_MARGIN_MIN).max(LOGO_MARGIN_MAX).optional(),
    borderRadius: z.number().int().min(LOGO_BORDER_RADIUS_MIN).max(LOGO_BORDER_RADIUS_MAX).optional(),
    backgroundColor: z.string().trim().min(LOGO_BG_COLOR_MIN_LENGTH).max(LOGO_BG_COLOR_MAX_LENGTH).optional(),
  }).strict().optional(),
  gradient: z.object({
    enabled: z.boolean().optional(),
    colors: z.tuple([colorSchema, colorSchema]).optional(),
    direction: z.tuple([
      z.number().min(GRADIENT_DIRECTION_MIN).max(GRADIENT_DIRECTION_MAX),
      z.number().min(GRADIENT_DIRECTION_MIN).max(GRADIENT_DIRECTION_MAX),
      z.number().min(GRADIENT_DIRECTION_MIN).max(GRADIENT_DIRECTION_MAX),
      z.number().min(GRADIENT_DIRECTION_MIN).max(GRADIENT_DIRECTION_MAX),
    ]).optional(),
  }).strict().optional(),
}).strict();

export type UpdateQrDesignRequest = z.infer<typeof UpdateQrDesignRequestSchema>;
