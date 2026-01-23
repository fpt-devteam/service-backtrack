export const DevicePlatform = {
  iOS: 'ios',
  Android: 'android',
} as const

export type DevicePlatformType = (typeof DevicePlatform)[keyof typeof DevicePlatform]
