import dotenv from 'dotenv';
import path from "node:path";

const envPath = path.resolve(process.cwd(), "../env/backtrack-qr-api.docker.env");

dotenv.config({
    path: envPath,
});

await import('@/src/configs/env.js');
await import("./main.js");