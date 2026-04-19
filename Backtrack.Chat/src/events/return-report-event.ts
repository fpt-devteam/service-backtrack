export interface ReturnReportSyncEvent {
    C2CReturnReportId: string;

    FinderId: string;
    FinderDisplayName: string | null;
    FinderAvatarUrl: string | null;
    FinderEmail: string | null;

    OwnerId: string;
    OwnerDisplayName: string | null;
    OwnerAvatarUrl: string | null;
    OwnerEmail: string | null;

    FinderPostId: string | null;
    FinderPostType: string | null;
    OwnerPostId: string | null;
    OwnerPostType: string | null;

    /** 'Draft' | 'Active' | 'Confirmed' | 'Rejected' | 'Expired' */
    Status: string;
    /** 'Finder' | 'Owner' | null */
    ActivatedByRole: string | null;
    ConfirmedAt: string | null;
    ExpiresAt: string;
    CreatedAt: string;
    EventTimestamp: string;
}
