import logger from '@src/utils/logger'

export interface ExpoPushMessage {
  to: string
  title?: string | null
  body?: string | null
  data?: Record<string, unknown>
  sound?: 'default' | null
}

export interface ExpoPushSuccessResult {
  status: 'ok'
  id: string
}

export interface ExpoPushErrorResult {
  status: 'error'
  message: string
  details?: unknown
}

export type ExpoPushResult = ExpoPushSuccessResult | ExpoPushErrorResult

export interface ExpoPushTicket {
  data: ExpoPushResult[]
}

export interface TokenPushResult {
  token: string
  success: boolean
  message?: string
  ticketId?: string
}

/**
 * Provider for sending push notifications via Expo Push API
 */
class ExpoPushProvider {
  private readonly EXPO_PUSH_URL = 'https://exp.host/--/api/v2/push/send'

  /**
   * Send push notifications to multiple Expo push tokens
   * @param tokens - Array of Expo push tokens
   * @param message - Message payload (title, body, data)
   * @returns Array of results per token
   */
  async sendToTokens(
    tokens: string[],
    message: {
      title?: string | null
      body?: string | null
      data?: Record<string, unknown>
    },
  ): Promise<TokenPushResult[]> {
    if (!tokens || tokens.length === 0) {
      return []
    }

    try {
      // Build messages for each token
      const messages: ExpoPushMessage[] = tokens.map((token) => ({
        to: token,
        sound: 'default',
        title: message.title || undefined,
        body: message.body || undefined,
        data: message.data || {},
      }))

      logger.info(`Sending push to ${tokens.length} tokens via Expo`, {
        tokenCount: tokens.length,
      })

      const response = await fetch(this.EXPO_PUSH_URL, {
        method: 'POST',
        headers: {
          Accept: 'application/json',
          'Accept-encoding': 'gzip, deflate',
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(messages),
      })

      if (!response.ok) {
        const errorText = await response.text()
        logger.error('Expo Push API error response', {
          status: response.status,
          body: errorText,
        })
        throw new Error(`Expo API returned status ${response.status}`)
      }

      const result = (await response.json()) as ExpoPushTicket

      // Map results back to tokens
      const results: TokenPushResult[] = tokens.map((token, index) => {
        const ticketResult = result.data[index]

        if (!ticketResult) {
          return {
            token,
            success: false,
            message: 'No result from Expo API',
          }
        }

        if (ticketResult.status === 'ok') {
          return {
            token,
            success: true,
            ticketId: ticketResult.id,
          }
        } else {
          return {
            token,
            success: false,
            message: ticketResult.message,
          }
        }
      })

      logger.info('Expo push results', {
        total: results.length,
        successful: results.filter((r) => r.success).length,
        failed: results.filter((r) => !r.success).length,
      })

      return results
    } catch (error) {
      logger.error('Failed to send push via Expo', { error })

      // Return all as failed
      return tokens.map((token) => ({
        token,
        success: false,
        message: error instanceof Error ? error.message : 'Unknown error',
      }))
    }
  }
}

export default new ExpoPushProvider()
