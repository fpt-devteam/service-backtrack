import mongoose from "mongoose";

export interface IPackage {
  _id: mongoose.Types.ObjectId;
  name: string;           
  qrCount: number;        
  price: number;          
  description?: string;
  isActive: boolean;  
  createdAt: Date;
  updatedAt: Date;
}

const PackageSchema = new mongoose.Schema<IPackage>(
  {
    name: { 
      type: String, 
      required: true,
      trim: true,
      unique: true,  
    },
    qrCount: { 
      type: Number, 
      required: true,
      min: 1,
    },
    price: { 
      type: Number, 
      required: true,
      min: 0,
    },
    description: { 
      type: String,
      trim: true,
    },
    isActive: { 
      type: Boolean, 
      default: true,
      index: true,  
    },
  },
  {
    timestamps: true,
  }
);

PackageSchema.index({ isActive: 1, displayOrder: 1 });  
PackageSchema.index({ qrCount: 1 });                    
PackageSchema.index({ price: 1 });

export const PackageModel = mongoose.model<IPackage>('Package', PackageSchema);