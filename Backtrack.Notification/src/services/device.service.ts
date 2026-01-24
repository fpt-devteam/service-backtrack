import { UpdatePushTokenRequest } from '@src/contracts/requests/device.request'
import deviceRepository from '@src/repositories/device.repository'
import { ErrorCodes } from '@src/common/errors/error.constants'

class DeviceService {
  public async registerDevice(userId: string, data: UpdatePushTokenRequest) {
    const result = await deviceRepository.upsertDevice(userId, data)
    return result
  }

  public async unregisterDevice(userId: string, data: UpdatePushTokenRequest) {
    const { deviceId } = data
    const result = await deviceRepository.deactivateDevice(userId, deviceId)

    if (!result) throw ErrorCodes.DeviceNotFound
    return result
  }
}

export default new DeviceService()
