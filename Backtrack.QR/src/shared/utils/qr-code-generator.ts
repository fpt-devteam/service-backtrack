/**
 * Generates a unique public code for QR codes
 * Format: BTK-XXXXXXXX (8 alphanumeric characters after prefix)
 */
export const generatePublicCode = (): string => {
    const prefix = 'BTK';
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    const length = 8;

    let code = '';
    for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * characters.length);
        code += characters[randomIndex];
    }

    return `${prefix}-${code}`;
};
