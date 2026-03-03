import { z } from "zod";

const EnvSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  PORT: z.coerce.number().int().min(1).max(65535).default(4000),
  HOST: z.string().default("localhost"),
  CLIENT_URL: z.string().url().default("http://localhost:3000"),
  LOG_LEVEL: z.enum(["error", "warn", "info", "debug"]).default("info"),
  MONGODB_CONNECTIONSTRING: z.string().min(1, "MONGODB_CONNECTIONSTRING is required"),
  RABBITMQ_URL: z.string().url("RABBITMQ_URL must be a valid URL"),
  RABBITMQ_EXCHANGE: z.string().min(1, "RABBITMQ_EXCHANGE is required"),
  RABBITMQ_USER_SYNC_QUEUE: z.string().min(1, "RABBITMQ_USER_SYNC_QUEUE is required"),
  // Firebase Admin SDK — same base64-encoded service account JSON used by the API Gateway
  FIREBASE_SERVICE_ACCOUNT_JSON_BASE64: z.string().optional(),
});

type Env = z.infer<typeof EnvSchema>;

function formatZodError(err: z.ZodError) {
  return err.issues
    .map((i) => `- ${i.path.join(".") || "(root)"}: ${i.message}`)
    .join("\n");
}

const parsed = EnvSchema.safeParse(process.env);

if (!parsed.success) {
  console.error("Invalid environment variables:\n" + formatZodError(parsed.error));
  process.exit(1);
}

export const env: Readonly<Env> = Object.freeze(parsed.data);

// Helper computed values
export const isDevelopment = env.NODE_ENV === "development";
export const isProduction = env.NODE_ENV === "production";
export const isTest = env.NODE_ENV === "test";
