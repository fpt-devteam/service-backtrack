import express from 'express'
import DeviceController from '@src/controllers/device.controller'
import { validateBody } from '@src/middlewares/validation.middleware'
import { RegisterDeviceBodySchema, UnregisterDeviceBodySchema } from '@src/contracts/requests/device.request'

const router = express.Router()

router.post('/register', validateBody(RegisterDeviceBodySchema), DeviceController.registerDevice.bind(DeviceController))
router.post('/unregister', validateBody(UnregisterDeviceBodySchema), DeviceController.unregisterDevice.bind(DeviceController))

export default router
