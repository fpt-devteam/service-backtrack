import dotenv from 'dotenv';
import path from "node:path";
import fs from "node:fs";

const envPath = path.resolve(process.cwd(), "../env/backtrack-qr-api.docker.env");

console.log("cwd =", process.cwd());
console.log("dotenv path resolved =", envPath);
console.log("exists =", fs.existsSync(envPath));

dotenv.config({
    path: envPath,
});

const { env } = await import('@/src/configs/env.js');
console.log("DATABASE_URI loaded?", Boolean(process.env.DATABASE_URI));
console.log("PORT loaded?", Boolean(process.env.PORT));
console.log("PORT, env.PORT =", env.PORT);

await import("./main.js");