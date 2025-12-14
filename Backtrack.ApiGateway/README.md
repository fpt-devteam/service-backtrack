# BackTrack API Gateway

Production-ready API Gateway built with .NET 8 and YARP (Yet Another Reverse Proxy) featuring Firebase Authentication for the BackTrack microservices system.

## Quick Start

```bash
# 1. Set Firebase credentials
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/firebase-service-account.json"

# 2. Run the gateway
cd Backtrack.ApiGateway
dotnet run

# 3. Test health endpoint
curl http://localhost:5000/health
```

## Architecture

**Single entry point** for all client requests with:
- **Firebase Authentication** - Validates ID tokens, injects user headers
- **Request Routing** - Routes to Core (8080), Chat (3000), Notifications (7000)
- **Correlation Tracking** - Unique IDs propagated across services
- **Request Logging** - Centralized logging with correlation context

## Routing

| Path | Destination | Auth Required |
|------|-------------|---------------|
| `/health` | Health check | No |
| `/public/**` | Public endpoints | No |
| `/api/core/**` | BackTrack.Core:8080 | Yes |
| `/api/chat/**` | BackTrack.Chat:3000 | Yes |
| `/api/notify/**` | BackTrack.Notifications:7000 | Yes |

## Configuration

### 1. Firebase Setup

Get service account credentials:
- Firebase Console > Project Settings > Service Accounts
- Click "Generate New Private Key"
- Save as `firebase-service-account.json` (DO NOT commit!)

Configure in `appsettings.Development.json`:
```json
{
  "Firebase": {
    "ProjectId": "your-firebase-project-id",
    "ServiceAccountPath": "/path/to/firebase-service-account.json",
    "ForwardAuthHeader": false
  }
}
```

Or use environment variable:
```bash
export GOOGLE_APPLICATION_CREDENTIALS="/path/to/firebase-service-account.json"
```

### 2. Backend Services

Edit `appsettings.Development.json` for local development:
```json
{
  "ReverseProxy": {
    "Clusters": {
      "core-cluster": {
        "Destinations": {
          "core-destination": {
            "Address": "http://localhost:8080"
          }
        }
      }
    }
  }
}
```

Production (Docker) uses service names like `http://backtrack-core:8080`.

## Authentication Flow

**1. Client Request:**
```bash
GET /api/core/posts
Authorization: Bearer <FIREBASE_ID_TOKEN>
```

**2. Gateway Processing:**
- Validates Firebase token
- Injects headers for downstream services:
  - `X-User-Id`: Firebase UID
  - `X-User-Email`: User email
  - `X-User-Name`: Display name
  - `X-Auth-Provider`: "firebase"
  - `X-Correlation-Id`: Request tracking ID

**3. Downstream Request:**
```bash
GET /posts
X-User-Id: abc123
X-User-Email: user@example.com
X-Correlation-Id: 550e8400-e29b...
```

## Getting Firebase Tokens

### Option 1: HTML Auth Client (Recommended)

```bash
# Open test-client.html with VS Code Live Server
# Login with Email/Password or Google
# Copy token for use in Postman
```

See `examples/README.md` for details.

### Option 2: Node.js CLI

```bash
cd examples
npm install
node get-firebase-token.js user@example.com password123
```

## Running the Gateway

### Local Development

```bash
cd Backtrack.ApiGateway
dotnet restore
dotnet run
# Gateway starts at http://localhost:5000

# Or with hot-reload
dotnet watch run
```

### Docker

```bash
# Build
docker build -t backtrack-gateway:latest -f Backtrack.ApiGateway/Dockerfile .

# Run
docker run -p 5000:80 \
  -e Firebase__ProjectId="your-project-id" \
  -v /path/to/firebase-service-account.json:/app/firebase-credentials.json:ro \
  -e Firebase__ServiceAccountPath="/app/firebase-credentials.json" \
  backtrack-gateway:latest
```

### Docker Compose

```bash
# Configure environment
cp .env.example .env
# Edit .env with your values

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f api-gateway

# Stop
docker-compose down
```

## API Examples

### Public Endpoint (No Auth)
```bash
curl http://localhost:5000/health
# Response: Healthy
```

### Secured Endpoint
```bash
export FIREBASE_TOKEN="eyJhbGciOiJSUzI1NiIsImtpZCI6IjExMjM0..."

curl http://localhost:5000/api/core/posts \
  -H "Authorization: Bearer $FIREBASE_TOKEN"
```

### With Correlation ID
```bash
curl http://localhost:5000/api/core/posts \
  -H "Authorization: Bearer $FIREBASE_TOKEN" \
  -H "X-Correlation-Id: my-custom-id" \
  -v
```

## Project Structure

```
Backtrack.ApiGateway/
├── Middleware/
│   ├── FirebaseAuthMiddleware.cs       # Token validation, user injection
│   └── CorrelationIdMiddleware.cs      # Request tracking
├── examples/
│   ├── test-client.html                # Auth client (Email/Google)
│   ├── get-firebase-token.js           # CLI token helper
│   └── README.md                       # Examples documentation
├── Program.cs                          # Entry point, YARP config
├── appsettings.json                    # Production config
├── appsettings.Development.json        # Dev config (localhost)
├── Dockerfile                          # Container image
└── README.md                           # This file
```

## Dependencies

```xml
<PackageReference Include="FirebaseAdmin" Version="3.0.1" />
<PackageReference Include="Yarp.ReverseProxy" Version="2.2.0" />
```

## Security Features

✅ Firebase token validation via Admin SDK
✅ Public endpoint exemption (`/health`, `/public/*`)
✅ User identity injection to downstream services
✅ Configurable auth header forwarding (default: off)
✅ Correlation ID generation and propagation
✅ No hardcoded secrets
✅ Multiple credential loading methods
✅ Graceful error handling

## Troubleshooting

### "Failed to initialize Firebase"
- Verify service account path is correct
- Check JSON file is valid
- Ensure `Firebase.ProjectId` matches your project

### "503 Service Unavailable"
- Backend services not running
- Check service URLs in `appsettings.json`
- Verify network connectivity

### "401 Unauthorized"
- Token expired (1 hour lifetime) - get fresh token
- Verify header format: `Authorization: Bearer <token>`
- Check endpoint requires authentication

### Add Public Endpoints
Edit `Middleware/FirebaseAuthMiddleware.cs`:
```csharp
_publicPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "/health",
    "/public",
    "/my-new-public-endpoint"  // Add here
};
```

## Extensibility

1. **Add Routes**: Update `appsettings.json` ReverseProxy section
2. **Add Middleware**: Insert in `Program.cs` pipeline
3. **Add Public Endpoints**: Modify `FirebaseAuthMiddleware._publicPaths`
4. **Add Headers**: Extend `FirebaseAuthMiddleware`

## Production Checklist

- [ ] Use HTTPS/TLS certificates
- [ ] Set `ForwardAuthHeader: false` in production
- [ ] Implement rate limiting middleware
- [ ] Set up centralized logging (Elasticsearch, Application Insights)
- [ ] Configure CORS policies if needed
- [ ] Set up monitoring and alerting
- [ ] Rotate Firebase service account keys regularly
- [ ] Enable health checks
- [ ] Configure load balancing for multiple instances

## Resources

- [Examples Documentation](examples/README.md) - Auth client, token helpers
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [Firebase Admin SDK](https://firebase.google.com/docs/admin/setup)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)

---

**Target:** .NET 8 | **Proxy:** YARP 2.2.0 | **Auth:** Firebase Admin SDK 3.0.1
