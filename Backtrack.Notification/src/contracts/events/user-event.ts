export interface UserEnsureExistEvent {
  Id: string;
  Email?: string;
  DisplayName?: string;
  AvatarUrl?: string | null;
  GlobalRole: string;
  CreatedAt: string;
  EventTimestamp: string;
}
