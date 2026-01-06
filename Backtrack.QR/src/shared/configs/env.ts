import { z } from "zod";

const EnvSchema = z.object({
    NODE_ENV: z.enum(["development", "test", "production"]).default("development"),

    PORT: z.coerce.number().int().min(1).max(65535).default(3000),

    DATABASE_URI: z.string().min(1, "DATABASE_URI is required"),
    RABBITMQ_URL: z.string().url("RABBITMQ_URL must be a valid URL"),
    RABBITMQ_EXCHANGE: z.string().min(1, "RABBITMQ_EXCHANGE is required"),
    RABBITMQ_USER_SYNC_QUEUE: z.string().min(1, "RABBITMQ_USER_SYNC_QUEUE is required"),
    APP_URL: z.string().url("APP_URL must be a valid URL"),
    LOG_LEVEL: z.enum(["debug", "info", "warn", "error"]).default("info"),
    PAYOS_CLIENT_ID: z.string().min(1, "PAYOS_CLIENT_ID is required"),
    PAYOS_API_KEY: z.string().min(1, "PAYOS_API_KEY is required"),
    PAYOS_CHECKSUM_KEY: z.string().min(1, "PAYOS_CHECKSUM_KEY is required"),
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
