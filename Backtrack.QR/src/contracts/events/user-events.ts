export interface UserCreatedEvent {
    Id: string;
    Email: string;
    DisplayName?: string;
    CreatedAt: string;
    EventTimestamp: string;
}

export interface UserUpdatedEvent {
    Id: string;
    Email?: string;
    DisplayName?: string;
    UpdatedAt: string;
    EventTimestamp: string;
}
