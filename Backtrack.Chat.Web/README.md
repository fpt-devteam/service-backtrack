# Backtrack.Chat.Web - Chat Client Application

Frontend client for the Backtrack Chat Service. A lightweight, vanilla JavaScript chat application with real-time messaging powered by Socket.IO.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Usage](#usage)
- [Development](#development)

## Overview

Backtrack.Chat.Web is the frontend client for the Backtrack Chat microservice. It provides a clean, modern chat interface built with vanilla JavaScript, HTML, and CSS - no build process required!

## Features

- **Real-time Messaging**: Instant message delivery via Socket.IO
- **Conversation Management**: Create and manage single and group chats
- **User-Friendly Interface**: Clean, responsive design
- **Local Development**: Simple HTTP server for testing
- **No Build Step**: Pure HTML/CSS/JS - works out of the box
- **Persistent Login**: LocalStorage-based authentication

## Tech Stack

- **HTML5**: Semantic markup
- **CSS3**: Modern styling with CSS variables
- **Vanilla JavaScript**: No frameworks needed
- **Socket.IO Client**: Real-time WebSocket communication
- **http-server**: Simple development server

## Project Structure

```
Backtrack.Chat.Web/
├── index.html                  # Landing page (redirects to login)
├── login.html                  # Login page
├── chat.html                   # Main chat interface
├── config.js                   # API configuration
├── package.json                # NPM dependencies
├── README.md                   # This file
│
├── src/
│   ├── scripts/                # JavaScript files
│   │   ├── login.js            # Login logic
│   │   ├── chat.js             # Main chat functionality
│   │   └── http.js             # HTTP request utilities
│   │
│   └── styles/                 # CSS files
│       └── chat.css            # Main styles
│
└── public/                     # Static assets (if needed)
```

## Prerequisites

- **Node.js**: v14+ (only for development server)
- **Backend API**: Backtrack.Chat service running on `http://localhost:3000`
- **Modern Browser**: Chrome, Firefox, Safari, or Edge

## Installation

1. **Navigate to the project directory**:
   ```bash
   cd Backtrack.Chat.Web
   ```

2. **Install dependencies** (for development server):
   ```bash
   npm install
   ```

That's it! No build step needed.

## Configuration

### API Configuration

Edit `config.js` to point to your backend API:

```javascript
const API_CONFIG = {
  // Change this to your backend API URL
  BASE_URL: 'http://localhost:3000',

  // Socket.IO URL (usually same as base URL)
  SOCKET_URL: 'http://localhost:3000',

  // API endpoints (configured automatically)
  ENDPOINTS: {
    CONVERSATIONS: '/api/conversations',
    MESSAGES: (conversationId) => `/api/conversations/${conversationId}/messages`,
  }
};
```

**For production:**
```javascript
BASE_URL: 'https://your-api-gateway.com/chat',
SOCKET_URL: 'https://your-api-gateway.com/chat',
```

## Running the Application

### Development Mode

**Option 1: Using npm script** (recommended):
```bash
npm run dev
```
This will start a server on `http://localhost:8000` and open your browser.

**Option 2: Using npx**:
```bash
npx http-server ./ -p 8000 -o
```

**Option 3: Using Python**:
```bash
# Python 3
python -m http.server 8000

# Python 2
python -m SimpleHTTPServer 8000
```

### Production Deployment

Serve the files using any static file server:

**Nginx example**:
```nginx
server {
    listen 80;
    server_name chat.example.com;
    root /path/to/Backtrack.Chat.Web;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

**Apache example**:
```apache
<VirtualHost *:80>
    ServerName chat.example.com
    DocumentRoot /path/to/Backtrack.Chat.Web

    <Directory /path/to/Backtrack.Chat.Web>
        Options Indexes FollowSymLinks
        AllowOverride All
        Require all granted
    </Directory>
</VirtualHost>
```

**Docker example**:
```dockerfile
FROM nginx:alpine
COPY . /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

## Usage

### Login Process

1. Open the application in your browser
2. Enter a username from the available users:
   - Alice (ID: `693d82480025a5c1d8f5b33e`)
   - Bob (ID: `693d82480025a5c1d8f5b33f`)
   - Charlie (ID: `693d82480025a5c1d8f5b340`)
   - Diana (ID: `693d82480025a5c1d8f5b341`)
3. Click "Đăng nhập" or click on a user tag

### Creating Conversations

1. Click the **+** button in the sidebar
2. Select conversation type:
   - **Single**: Private 1-on-1 chat
   - **Group**: Group chat with multiple users
3. Select participants from the list
4. For groups, enter a group name
5. Click "Tạo" to create

### Sending Messages

1. Select a conversation from the sidebar
2. Type your message in the input box
3. Press Enter or click "Gửi"
4. Messages appear instantly via Socket.IO

### Real-time Features

- **Live Messages**: See messages as they arrive
- **Active Conversations**: Visual feedback for selected chat
- **Typing Indicators**: (Future feature)
- **Read Receipts**: (Future feature)

## Development

### Adding Features

The codebase is structured for easy modification:

**1. Update UI** (`chat.html`, `login.html`):
```html
<!-- Add new elements -->
<button id="new-feature-btn">New Feature</button>
```

**2. Add Styles** (`src/styles/chat.css`):
```css
.new-feature {
    /* Your styles */
}
```

**3. Add Logic** (`src/scripts/chat.js`):
```javascript
// Add event listeners
document.getElementById('new-feature-btn')
    .addEventListener('click', handleNewFeature);

function handleNewFeature() {
    // Implementation
}
```

### API Integration

All API calls use the `fetch` API and follow this pattern:

```javascript
async function apiCall() {
    try {
        const response = await fetch('/api/endpoint', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Auth-Id': state.currentUserId,
            },
            body: JSON.stringify({ data }),
        });

        const result = await response.json();

        if (!result.success) {
            throw new Error(result.error?.message || 'Request failed');
        }

        return result.data;
    } catch (error) {
        console.error('API error:', error);
        showError(error.message);
    }
}
```

### Socket.IO Integration

Real-time messaging is handled via Socket.IO:

```javascript
// Initialize socket
socket = io('http://localhost:3000');

// Join conversation room
socket.emit('join_conversation', conversationId);

// Listen for messages
socket.on('receive_message', (message) => {
    state.messages.push(message);
    renderMessage(message);
});
```

### State Management

Application state is managed via a simple state object:

```javascript
const state = {
    currentUserId: null,
    currentConversationId: null,
    conversations: [],
    messages: [],
    users: [],
};
```

### Error Handling

All errors include correlation IDs for debugging:

```javascript
if (!result.success) {
    const error = new Error(result.error?.message);
    error.correlationId = result.correlationId;
    throw error;
}
```

## Browser Compatibility

- Chrome/Edge: ✅ Latest 2 versions
- Firefox: ✅ Latest 2 versions
- Safari: ✅ Latest 2 versions
- IE11: ❌ Not supported

## Security Considerations

- This is a demo application with hardcoded users
- In production, implement proper authentication:
  - JWT tokens
  - OAuth2/OIDC
  - Session management
- Always use HTTPS in production
- Implement CORS properly on the backend
- Sanitize user input
- Validate all API responses

## Troubleshooting

### Cannot connect to API
```
Error: Failed to fetch
```
**Solution**: Check `config.js` and ensure backend is running on the correct port.

### Socket.IO not connecting
```
Error: WebSocket connection failed
```
**Solution**:
- Verify `SOCKET_URL` in `config.js`
- Check backend Socket.IO CORS configuration
- Ensure backend is running

### CORS errors
```
Access to fetch blocked by CORS policy
```
**Solution**: Configure CORS on the backend:
```javascript
app.use(cors({
    origin: 'http://localhost:8000',
    credentials: true
}));
```

## Future Enhancements

- [ ] TypeScript migration
- [ ] Build process with bundling
- [ ] PWA support (offline mode)
- [ ] File/image sharing
- [ ] Voice/video calls
- [ ] Message reactions
- [ ] Typing indicators
- [ ] Read receipts
- [ ] Push notifications
- [ ] Dark mode
- [ ] Emoji picker
- [ ] Message search
- [ ] User profiles

## Contributing

1. Make changes to HTML/CSS/JS files
2. Test in the browser
3. No build step needed!
4. Submit pull request

## License

ISC

## Support

For issues or questions about the frontend client, please create an issue in the repository.

## Related Projects

- **Backtrack.Chat**: Backend API service
- **Backtrack.QR**: QR code management service
- **Backtrack.Core**: Core business logic service
