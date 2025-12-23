export type Error = {
    kind: "Validation" | "NotFound" | "Unauthorized" | "Conflict" | "Internal";
    code: string;
    message: string;
    details?: string;
};
