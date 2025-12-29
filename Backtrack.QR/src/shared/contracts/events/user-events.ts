export interface UserUpsertedEvent {
  Id: string;
  Email?: string;
  DisplayName?: string;
  GlobalRole: string;
  CreatedAt: string;
  EventTimestamp: string;
}
