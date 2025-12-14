# BackTrack API Gateway - Authentication Client

## 📋 Overview

This directory contains tools for Firebase authentication and token generation:

- **test-client.html** - HTML authentication client (Email/Password + Google Sign-In)
- **get-firebase-token.js** - Node.js script to retrieve Firebase tokens via CLI

**Note:** API testing is done with **Postman** or similar tools. This client only handles authentication and token generation.

## 🌐 HTML Authentication Client

### Features

- ✅ Firebase Email/Password authentication
- ✅ Google Sign-In integration
- ✅ Real-time token display and copying
- ✅ Clean, focused UI for authentication only
- ✅ Token expiration notice
- ✅ One-click copy to clipboard

### Setup Instructions

#### Step 1: Enable Authentication in Firebase

**Enable Email/Password:**
1. Firebase Console > Authentication > Sign-in method
2. Enable "Email/Password"
3. Save

**Enable Google Sign-In:**
1. Firebase Console > Authentication > Sign-in method
2. Enable "Google"
3. Add authorized domains (e.g., `localhost`, `127.0.0.1`)
4. Save

#### Step 2: Create Test User

1. Firebase Console > Authentication > Users
2. Click "Add user"
3. Enter email and password
4. Click "Add user"

#### Step 3: Run the Client

**Using VS Code Live Server Extension (Recommended):**

1. Install "Live Server" extension in VS Code:
   - Open VS Code
   - Go to Extensions (Ctrl+Shift+X)
   - Search for "Live Server" by Ritwick Dey
   - Click Install

2. Open the authentication client:
   - Right-click on `test-client.html`
   - Select "Open with Live Server"
   - Browser will open automatically at `http://127.0.0.1:5500/test-client.html`

**Alternative: Open Directly in Browser**
```bash
# Just open the file in your browser
open test-client.html  # macOS
start test-client.html # Windows
xdg-open test-client.html # Linux
```

> **Note:** Live Server is recommended as it provides auto-reload and proper MIME types for Firebase SDK.

### Using the Authentication Client

#### 1. Login

**Email/Password:**
1. Click "📧 Email/Password Login"
2. Enter your email and password
3. Click "Login" or press Enter

**Google Sign-In:**
1. Click "Sign in with Google"
2. Select your Google account
3. Authorize the app

#### 2. Copy Your Token

After successful login:
- User information is displayed (ID, Email, Name)
- Firebase ID token is shown in a scrollable box
- Click "📋 Copy Token to Clipboard"
- Token is ready to paste into Postman!

#### 3. Use in Postman

1. Open Postman
2. Create a new request
3. Go to "Headers" tab
4. Add header:
   - Key: `Authorization`
   - Value: `Bearer <PASTE_YOUR_TOKEN_HERE>`
5. Send your request to the API Gateway

#### 4. Token Expiration

- Tokens expire after 1 hour
- Simply logout and login again to get a fresh token
- Or refresh the page and login again

### Example Postman Setup

**Request:**
```
GET http://localhost:5000/api/core/posts
```

**Headers:**
```
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjExMjM0...
Content-Type: application/json
```

**Response:**
```json
{
  "posts": [...]
}
```

## 🔧 Node.js Token Helper (CLI)

### Setup
```bash
cd examples
npm install
```

### Usage
```bash
node get-firebase-token.js user@example.com password123
```

This will output:
- User ID
- Email
- Display Name
- Firebase ID Token
- Export command for bash
- Example curl command
- Token expiration time

### Example Output
```
========================================
Firebase ID Token Retrieved Successfully
========================================

User ID: abc123xyz
Email: user@example.com
Display Name: John Doe

--- Firebase ID Token ---
eyJhbGciOiJSUzI1NiIsImtpZCI6IjExMjM0...

--- Export to Environment ---
export FIREBASE_TOKEN="eyJhbGciOiJSUzI1NiIsImtpZCI6IjExMjM0..."

--- Example curl command ---
curl -X GET http://localhost:5000/api/core/posts \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjExMjM0..."

========================================

Token expires at: 12/14/2025, 3:45:30 PM
Token valid for: 59 minutes
```

## 📊 Comparison

| Tool | Best For | Pros | Cons |
|------|----------|------|------|
| **test-client.html** | Interactive authentication | Visual UI, Google Sign-In, easy to use | Requires browser |
| **get-firebase-token.js** | Quick CLI token retrieval | Fast, scriptable, automation | Node.js required |

## 🔒 Security Notes

1. **Never commit Firebase tokens** - They expire after 1 hour anyway
2. **Keep Firebase config safe** - Already configured in HTML file
3. **Use test users** - Create separate test accounts for development
4. **Localhost only** - These tools are for local development only
5. **HTTPS in production** - Always use HTTPS for production testing

## 📚 Postman Collections

### Suggested Postman Collection Structure

```
BackTrack API Gateway
├── Authentication
│   └── Health Check (Public)
├── Core Service
│   ├── Get All Posts
│   ├── Get Post by ID
│   ├── Create Post
│   ├── Update Post
│   └── Delete Post
├── Chat Service
│   ├── Get Conversations
│   ├── Send Message
│   └── Get Messages
└── Notifications Service
    ├── Register FCM Token
    ├── Get Notifications
    └── Mark as Read
```

### Postman Environment Variables

Create a Postman environment with:
```
GATEWAY_URL: http://localhost:5000
FIREBASE_TOKEN: <paste-from-auth-client>
```

Then use:
```
{{GATEWAY_URL}}/api/core/posts
Authorization: Bearer {{FIREBASE_TOKEN}}
```

## 🆘 Troubleshooting

### Issue: Firebase Configuration Error

**Error:** `Firebase: Error (auth/invalid-api-key)`

**Solution:**
- Firebase config is already set in test-client.html
- If you need to change projects, update the config in the HTML file

### Issue: Google Sign-In Popup Blocked

**Solution:**
- Allow popups for localhost/127.0.0.1 in browser settings
- Try again after allowing popups

### Issue: Token Expired in Postman

**Solution:**
- Tokens expire after 1 hour
- Go back to test-client.html
- Logout and login again
- Copy fresh token to Postman

### Issue: 401 Unauthorized in Postman

**Solution:**
- Verify header format: `Authorization: Bearer <token>`
- Make sure there's a space after "Bearer"
- Check that token is not expired
- Ensure endpoint requires authentication

## 📝 Example Workflow

```bash
# Terminal: Start the API Gateway
cd Backtrack.ApiGateway
dotnet run
```

**VS Code:**
1. Open `examples/test-client.html`
2. Right-click and select "Open with Live Server"
3. Browser opens automatically at `http://127.0.0.1:5500/test-client.html`

**Authentication:**
1. Login with Email/Password or Google
2. Click "Copy Token to Clipboard"

**Postman:**
1. Open Postman
2. Create new request: `GET http://localhost:5000/api/core/posts`
3. Add header: `Authorization: Bearer <paste-token>`
4. Click Send
5. Test your APIs!

## 📚 Additional Resources

- [Firebase Authentication Docs](https://firebase.google.com/docs/auth)
- [Postman Documentation](https://learning.postman.com/docs/getting-started/introduction/)
- [YARP Documentation](https://microsoft.github.io/reverse-proxy/)
- [API Gateway README](../README.md)

Happy Testing! 🚀
