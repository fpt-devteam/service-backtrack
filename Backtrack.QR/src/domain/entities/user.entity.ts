import { UserGlobalRoleType } from "@/src/domain/constants/user-global-role.constant.js";

export type User = {
  id: string;
  email?: string | null;
  displayName?: string | null;
  avatarUrl?: string | null;
  globalRole: UserGlobalRoleType;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date | null;
  syncedAt: Date;
}
