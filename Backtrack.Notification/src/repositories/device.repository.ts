import { RegisterDeviceBody } from '@src/contracts/requests/device.request'
import { DeviceRegisterResult, DeviceUnregisterResult } from '@src/contracts/responses/device.response'
import { Device } from '@src/models/device.model'
import { Model } from 'mongoose'

class DeviceRepository {
  constructor(private readonly model: Model<any>) {}

  public async upsertDevice(userId: string, data: RegisterDeviceBody) {
    const now = new Date()
    const device = await this.model
      .findOneAndUpdate(
        { userId, deviceId: data.deviceId },
        {
          $set: {
            token: data.token,
            platform: data.platform,
            isActive: true,
            lastSeenAt: now,
            updatedAt: now,
          },
          $setOnInsert: {
            userId,
            deviceId: data.deviceId,
            createdAt: now,
          },
        },
        { upsert: true, new: true },
      )
      .exec()

    return {
      deviceId: device.deviceId,
      isActive: device.isActive,
      lastSeenAt: device.lastSeenAt,
    }
  }

  public async deactivateDeviceByTokenForOtherUsers(token: string, currentUserId: string) {
    // Mark all devices with this token (except current user) as inactive
    await this.model
      .updateMany(
        { token, userId: { $ne: currentUserId } },
        {
          $set: {
            isActive: false,
            updatedAt: new Date(),
          },
        },
      )
      .exec()
  }

  public async findDeviceByUserAndToken(userId: string, token: string) {
    return await this.model.findOne({ userId, token }).exec()
  }

  public async findDeviceByUserAndDeviceId(userId: string, deviceId: string) {
    return await this.model.findOne({ userId, deviceId }).exec()
  }

  public async deactivateDevice(userId: string, deviceIdentifier: { token?: string; deviceId?: string }) {
    const query: any = { userId }

    if (deviceIdentifier.deviceId) {
      query.deviceId = deviceIdentifier.deviceId
    } else if (deviceIdentifier.token) {
      query.token = deviceIdentifier.token
    } else {
      return null
    }

    const device = await this.model
      .findOneAndUpdate(
        query,
        {
          $set: {
            isActive: false,
            lastSeenAt: new Date(),
            updatedAt: new Date(),
          },
        },
        { new: true },
      )
      .exec()

    if (!device) {
      return null
    }

    return {
      deviceId: device.deviceId,
      isActive: device.isActive,
    }
  }
}

export default new DeviceRepository(Device)
