import { UserGlobalRole, UserGlobalRoleType } from "@/src/domain/constants/user-global-role.constant.js";
import { z } from "zod";

export type CreateUserRequest = {
  email?: string | null;
  displayName?: string | null;
  avatarUrl?: string | null;
  globalRole: UserGlobalRoleType;
};

export const CreateUserRequestSchema = z.object({
  email: z.email().optional().nullable(),
  displayName: z.string().trim().min(1, "displayName is required").max(100).optional().nullable(),
  avatarUrl: z.url().optional().nullable(),
  globalRole: z.enum(Object.values(UserGlobalRole) as [string, ...string[]]),

}).strict();
