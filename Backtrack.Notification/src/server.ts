import morgan from 'morgan'
import helmet from 'helmet'
import cors from 'cors'
import express, { Request, Response, Express } from 'express'
import { errorHandler } from '@src/middlewares/error-handler'
import ENV from '@src/common/constants/ENV'
import notificationRoute from '@src/routes/notification.route'

const app: Express = express()

// Middleware
app.use(cors())
app.use(express.json())
app.use(express.urlencoded({ extended: true }))
app.use(helmet())

if (ENV.NodeEnv === 'development') {
  app.use(morgan('dev'))
}

// Health check endpoint
app.get('/health', (_: Request, res: Response) => {
  res.json({ status: 'healthy' })
})

// API Routes
app.use('/', notificationRoute)

// 404 handler
app.use((req: Request, res: Response) => {
  res.status(404).json({
    success: false,
    error: {
      code: 'NotFound',
      message: `Cannot ${req.method} ${req.path}`,
    },
  })
})

// Error handler must be last
app.use(errorHandler)

export default app
