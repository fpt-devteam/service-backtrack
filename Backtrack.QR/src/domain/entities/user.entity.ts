import { UserGlobalRoleType } from "@/src/domain/constants/user-global-role.constant.js";

export type User = {
  id: string;
  email?: string | null;
  displayName?: string | null;
  avatarUrl?: string | null;
  globalRole: UserGlobalRoleType;
  providerCustomerId?: string | null;
  subscriptionStatus?: string | null;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date | null;
}
