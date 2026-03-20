import mongoose from 'mongoose'

const QRDesignSchema = new mongoose.Schema({
  size: { type: Number, default: 200 },

  color: { type: String, default: '#000000' },

  backgroundColor: { type: String, default: '#FFFFFF' },

  quietZone: { type: Number, default: 10 },

  ecl: {
    type: String,
    enum: ['L', 'M', 'Q', 'H'],
    default: 'H'
  },

  logo: {
    url: { type: String, default: '' },
    size: { type: Number, default: 50 },
    margin: { type: Number, default: 2 },
    borderRadius: { type: Number, default: 0 },
    backgroundColor: { type: String, default: 'transparent' }
  },

  gradient: {
    enabled: { type: Boolean, default: false },
    colors: {
      type: [String],
      default: ['#000000', '#000000'],
      validate: [(val: string[]) => val.length === 2, 'Gradient must have exactly 2 colors']
    },
    direction: {
      type: [Number],
      default: [0, 0, 1, 1],
      validate: [(val: number[]) => val.length === 4, 'Direction must have exactly 4 coordinates']
    }
  },

  userId: { type: String, required: true },
}, { timestamps: true });

export const QRDesign = mongoose.model('QRDesign', QRDesignSchema);

// await QRDesign.updateOne(
//   {
//     userId: "temp-user-1"
//   },
//   {
//     $set: {
//       size: 280, color: "#137fec", backgroundColor: "#FFFFFF", quietZone: 12, ecl: "H",
//       logo: { url: "", size: 58, margin: 3, borderRadius: 8, backgroundColor: "transparent" },
//       gradient: {
//         enabled: true, colors: ["#137fec", "#00b894"],
//         direction: [0, 0, 1, 1]
//       }
//     }
//   }, { upsert: true });

console.log('Default QR design created/updated for temp-user-1');