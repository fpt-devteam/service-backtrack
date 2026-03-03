import { UserGlobalRoleType } from '@/models/user.model';

export interface UserEnsureExistEvent {
  Id: string;
  Email?: string;
  DisplayName?: string;
  AvatarUrl?: string | null;
  GlobalRole: UserGlobalRoleType;
  CreatedAt: string;
  EventTimestamp: string;
}
