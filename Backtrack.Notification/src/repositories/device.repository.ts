import { UpdatePushTokenRequest } from '@src/contracts/requests/device.request'
import { Device } from '@src/models/device.model'
import { Model } from 'mongoose'

class DeviceRepository {
  constructor(private readonly model: Model<any>) {}

  public async upsertDevice(userId: string, data: UpdatePushTokenRequest) {
    const now = new Date()
    const { deviceId, token } = data

    const res = await this.model
      .findOneAndUpdate(
        { userId, deviceId },
        {
          $set: { token, isActive: true, lastSeenAt: now },
          $setOnInsert: { userId, deviceId },
        },
        { upsert: true, new: true, rawResult: true } as any,
      )
      .exec()

    const updatedExisting = res.lastErrorObject?.updatedExisting
    return { upserted: !updatedExisting }
  }

  public async deactivateDevice(userId: string, deviceId: string) {
    const now = new Date()

    const res = await this.model
      .findOneAndUpdate(
        { userId, deviceId, isActive: true },
        {
          $set: { isActive: false, lastSeenAt: now },
        },
        { new: true },
      )
      .lean()
      .exec()

    return {
      deactivated: !!res,
      device: res,
    }
  }
}

export default new DeviceRepository(Device)
