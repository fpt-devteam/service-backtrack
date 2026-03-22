import { QrDesignRepository, UpdateQrDesignFields } from '@/src/application/repositories/qr-design.repository.js';
import { QRDesign } from '@/src/infrastructure/database/models/qr-design.model.js';
import { qrDesignToDomain } from '@/src/infrastructure/database/mappers/qr-design.mapper.js';

const assignIfDefined = (target: Record<string, unknown>, key: string, value: unknown): void => {
  if (value !== undefined) target[key] = value;
};

const mapNestedFields = (
  target: Record<string, unknown>,
  prefix: string,
  fields: Record<string, unknown> | undefined
): void => {
  if (!fields) return;

  Object.entries(fields).forEach(([key, value]) => {
    assignIfDefined(target, `${prefix}.${key}`, value);
  });
};

const toMongoUpdate = (fields: UpdateQrDesignFields): Record<string, unknown> => {
  const updateFields: Record<string, unknown> = {};

  assignIfDefined(updateFields, 'size', fields.size);
  assignIfDefined(updateFields, 'color', fields.color);
  assignIfDefined(updateFields, 'backgroundColor', fields.backgroundColor);
  assignIfDefined(updateFields, 'quietZone', fields.quietZone);
  assignIfDefined(updateFields, 'ecl', fields.ecl);

  mapNestedFields(updateFields, 'logo', fields.logo as Record<string, unknown> | undefined);
  mapNestedFields(updateFields, 'gradient', fields.gradient as Record<string, unknown> | undefined);

  return updateFields;
};

export const createQrDesignRepository = (): QrDesignRepository => ({
  findByUserId: async (userId: string) => {
    const doc = await QRDesign.findOne({ userId });
    return doc ? qrDesignToDomain(doc) : null;
  },

  createDefaultForUser: async (userId: string) => {
    const doc = await QRDesign.create({ userId });
    return qrDesignToDomain(doc);
  },

  updateByUserId: async (userId: string, fields: UpdateQrDesignFields) => {
    const doc = await QRDesign.findOneAndUpdate(
      { userId },
      { $set: toMongoUpdate(fields) },
      { new: true }
    );

    return doc ? qrDesignToDomain(doc) : null;
  },
});
