import {
  RegisterDeviceBody,
  UnregisterDeviceBody,
} from '@src/contracts/requests/device.request'
import deviceRepository from '@src/repositories/device.repository'
import { ErrorCodes } from '@src/common/errors/error.constants'

class DeviceService {
  public async registerDevice(userId: string, data: RegisterDeviceBody) {
    // Handle token collision: deactivate other users' devices with the same token
    // This implements Policy A: reassign token by marking other records inactive
    await deviceRepository.deactivateDeviceByTokenForOtherUsers(
      data.token,
      userId,
    )

    // Upsert device by (userId, deviceId)
    const result = await deviceRepository.upsertDevice(userId, data)

    return result
  }

  public async unregisterDevice(userId: string, data: UnregisterDeviceBody) {
    const { token, deviceId } = data

    // Attempt to find device - prefer deviceId, fallback to token
    let device = null

    if (deviceId) {
      device = await deviceRepository.findDeviceByUserAndDeviceId(
        userId,
        deviceId,
      )
    }

    if (!device && token) {
      device = await deviceRepository.findDeviceByUserAndToken(userId, token)
    }

    // Ownership check: device must exist and belong to user
    if (!device) {
      throw ErrorCodes.DeviceNotFound
    }

    // Deactivate the device
    const result = await deviceRepository.deactivateDevice(userId, {
      token,
      deviceId,
    })

    if (!result) {
      throw ErrorCodes.DeviceNotFound
    }

    return result
  }
}

export default new DeviceService()
