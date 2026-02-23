export interface InvitationCreatedEvent {
  InvitationId: string;
  Email: string;
  OrganizationName: string;
  InviterName: string;
  Role: string;
  HashCode: string;
  ExpiredTime: string;
  EventTimestamp: string;
}