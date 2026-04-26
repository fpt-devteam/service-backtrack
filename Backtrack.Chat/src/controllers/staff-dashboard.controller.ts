import { Request, Response } from 'express';
import * as staffDashboardService from '@/services/staff-dashboard.service';
import { ApiResponseBuilder } from '@/utils/api-response';
import { Constants } from '@/config/constants';

const getCorrelationId = (req: Request) =>
    req.headers[Constants.HEADERS.CORRELATION_ID] as string;

export const getStaffChatStats = async (req: Request, res: Response) => {
    const staffId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const orgId   = req.headers[Constants.HEADERS.ORG_ID] as string;

    const stats = await staffDashboardService.getStaffChatStats(staffId, orgId);

    return res.status(200).json(
        ApiResponseBuilder.success(stats, getCorrelationId(req))
    );
};
