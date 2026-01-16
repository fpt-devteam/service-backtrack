export interface QrGenerationRequestedEvent {
    orderId: string;
    code: string;
    userId: string;
    qrCount: number;
    packageName: string;
    eventTimestamp: string;
}
