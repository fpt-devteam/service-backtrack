export type PagedRequest = {
    page?: number;
    pageSize?: number;
};

export type PagedResponse<T> = {
    items: T[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
};

export const createPagedResponse = <T>(
    items: T[],
    page: number,
    pageSize: number,
    totalCount: number
): PagedResponse<T> => {
    const totalPages = Math.ceil(totalCount / pageSize);

    return {
        items,
        page,
        pageSize,
        totalCount,
        totalPages
    };
};

export const DEFAULT_PAGE = 1;
export const DEFAULT_PAGE_SIZE = 20;
export const MAX_PAGE_SIZE = 100;


export const sanitizePageSize = (raw: unknown): number => {
    let n: number;
    if (typeof raw === "number") {
        n = raw;
    } else if (typeof raw === "string") {
        n = Number(raw.trim());
    } else {
        n = Number.NaN;
    }

    if (!Number.isFinite(n)) return DEFAULT_PAGE_SIZE;

    const v = Math.trunc(n);
    if (v < 1) return DEFAULT_PAGE_SIZE;      
    if (v > MAX_PAGE_SIZE) return MAX_PAGE_SIZE;
    return v;
};

export const sanitizePage = (raw: unknown): number => {
    let n: number;
    if (typeof raw === "number") {
        n = raw;
    } else if (typeof raw === "string") {
        n = Number(raw.trim());
    } else {
        n = Number.NaN;
    }

    if (!Number.isFinite(n)) return DEFAULT_PAGE;

    const v = Math.trunc(n);
    if (v < 1) return DEFAULT_PAGE;
    return v;
};