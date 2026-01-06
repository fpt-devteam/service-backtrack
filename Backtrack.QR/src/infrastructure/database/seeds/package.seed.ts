import mongoose from 'mongoose';
import { PackageModel } from '../models/package.model.js';


const MONGO_URI = 'mongodb://mongo:mongo123@localhost:27018/backtrack-qr-db?authSource=admin';

async function seedPackages() {
  try {
    await mongoose.connect(MONGO_URI);
    console.log('✅ Connected to MongoDB');

    const packages = [
      {
        name: 'Basic Package',
        qrCount: 5,
        price: 50000,
        description: 'Suitable for individuals',
        isActive: true,
      },
      {
        name: 'Popular Package',
        qrCount: 10,
        price: 90000,
        description: 'Most popular choice',
        isActive: true,
      },
      {
        name: 'Value Package',
        qrCount: 20,
        price: 160000,
        description: 'Best value for families',
        isActive: true,
      },
      {
        name: 'Business Package',
        qrCount: 50,
        price: 350000,
        description: 'Ideal solution for small businesses',
        isActive: true,
      },
    ];

    // 3️⃣ Clear old data
    await PackageModel.deleteMany({});
    console.log('🗑️ Old packages removed');

    // 4️⃣ Insert new data
    await PackageModel.insertMany(packages);
    console.log('🌱 Packages seeded successfully');

    process.exit(0);
  } catch (error) {
    console.error('❌ Seeding failed:', error);
    process.exit(1);
  }
}

seedPackages();
