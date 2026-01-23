import { Request, Response } from 'express'
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AsyncHandler } from '@src/decorators/async-handler'
import { HEADERS } from '@src/utils/headers'
import deviceService from '@src/services/device.service'
import {
  RegisterDeviceBodySchema,
  UnregisterDeviceBodySchema,
} from '@src/contracts/requests/device.request'
import {
  DeviceRegisterResponse,
  DeviceUnregisterResponse,
} from '@src/contracts/responses/device.response'

export class DeviceController {
  @AsyncHandler
  public async registerDevice(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const requestData = RegisterDeviceBodySchema.parse(req.body)

    const result = await deviceService.registerDevice(userId, requestData)

    const response: DeviceRegisterResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async unregisterDevice(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const requestData = UnregisterDeviceBodySchema.parse(req.body)

    const result = await deviceService.unregisterDevice(userId, requestData)

    const response: DeviceUnregisterResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }
}

export default new DeviceController()
