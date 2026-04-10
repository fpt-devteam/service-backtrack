import Org from '@/models/org.model';
import { IOrg } from '@/models/interfaces/org.interface';
import logger from '@/utils/logger';

export const ensureOrgExists = async (data: {
    id: string;
    name: string;
    slug: string;
    logoUrl: string;
}): Promise<IOrg> => {
    try {
        const org = await Org.findByIdAndUpdate(
            data.id,
            { $set: { name: data.name, slug: data.slug, logoUrl: data.logoUrl } },
            { upsert: true, new: true, setDefaultsOnInsert: true }
        ).lean().exec();

        logger.info(`Org ${data.id} ensured to exist`);
        return org!;
    } catch (error) {
        logger.error(`Failed to ensure org ${data.id} exists:`, { error: String(error) });
        throw error;
    }
};

export const getOrgById = async (id: string): Promise<IOrg | null> => {
    return Org.findById(id).lean().exec();
};
