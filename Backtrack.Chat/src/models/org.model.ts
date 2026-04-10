import { Schema, model } from 'mongoose';
import { IOrg } from './interfaces/org.interface';

const OrgSchema = new Schema<IOrg>(
    {
        _id: { type: String, required: true },
        name: { type: String, required: true },
        slug: { type: String, required: true },
        logoUrl: { type: String, required: true },
    },
    {
        timestamps: true,
        _id: false,
    }
);

const Org = model<IOrg>('Org', OrgSchema);
export default Org;
