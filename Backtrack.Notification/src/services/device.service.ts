import { RegisterDeviceBody, UnregisterDeviceBody } from '@src/contracts/requests/device.request'
import deviceRepository from '@src/repositories/device.repository'
import { ErrorCodes } from '@src/common/errors/error.constants'

class DeviceService {
  public async registerDevice(userId: string, data: RegisterDeviceBody) {
    await deviceRepository.deactivateDeviceByTokenForOtherUsers(data.token, userId)
    const result = await deviceRepository.upsertDevice(userId, data)
    return result
  }

  public async unregisterDevice(userId: string, data: UnregisterDeviceBody) {
    const { token, deviceId } = data

    let device = null
    if (deviceId) device = await deviceRepository.findDeviceByUserAndDeviceId(userId, deviceId)

    if (!device && token) device = await deviceRepository.findDeviceByUserAndToken(userId, token)

    if (!device) throw ErrorCodes.DeviceNotFound

    const result = await deviceRepository.deactivateDevice(userId, {
      token,
      deviceId,
    })

    if (!result) throw ErrorCodes.DeviceNotFound
    return result
  }
}

export default new DeviceService()
