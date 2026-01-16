import { env } from "../configs/env.js";

export const toVietnamISOString = (date: Date | null | undefined): string | null => {
    if (!date) return null;

    const vietnamDate = new Date(date.toLocaleString('en-US', { timeZone: env.VIETNAM_TIMEZONE }));
    const offset = '+07:00';

    const year = vietnamDate.getFullYear();
    const month = String(vietnamDate.getMonth() + 1).padStart(2, '0');
    const day = String(vietnamDate.getDate()).padStart(2, '0');
    const hours = String(vietnamDate.getHours()).padStart(2, '0');
    const minutes = String(vietnamDate.getMinutes()).padStart(2, '0');
    const seconds = String(vietnamDate.getSeconds()).padStart(2, '0');
    const milliseconds = String(vietnamDate.getMilliseconds()).padStart(3, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}.${milliseconds}${offset}`;
};

export const toVietnamISOStringOrDefault = (date: Date | null | undefined, defaultValue: string = new Date().toISOString()): string => {
    return toVietnamISOString(date) || defaultValue;
};
