import { Request, Response } from 'express'
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AsyncHandler } from '@src/decorators/async-handler'
import { HEADERS } from '@src/utils/headers'
import deviceService from '@src/services/device.service'
import { SyncDeviceRequestSchema } from '@src/contracts/requests/device.request'

export class DeviceController {
  @AsyncHandler
  public async registerDevice(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const requestData = SyncDeviceRequestSchema.parse(req.body)
    await deviceService.registerDevice(userId, requestData)
    return res.status(HTTP_STATUS_CODES.NoContent).json()
  }

  @AsyncHandler
  public async unregisterDevice(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const requestData = SyncDeviceRequestSchema.parse(req.body)
    await deviceService.unregisterDevice(userId, requestData)
    return res.status(HTTP_STATUS_CODES.NoContent).json()
  }
}

export default new DeviceController()
