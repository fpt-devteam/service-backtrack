export interface UserCreatedEvent {
  Id: string;
  Email: string;
  DisplayName?: string;
  AvatarUrl?: string | null;
  CreatedAt: string;
  EventTimestamp: string;
}

export interface UserUpdatedEvent {
  Id: string;
  Email?: string;
  DisplayName?: string;
  AvatarUrl?: string | null;
  UpdatedAt: string;
  EventTimestamp: string;
}
