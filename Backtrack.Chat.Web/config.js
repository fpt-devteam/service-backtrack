// API Configuration
const API_CONFIG = {
  // Change this to your backend API URL
  BASE_URL: 'http://localhost:3000',

  // Socket.IO URL (same as base URL for most cases)
  SOCKET_URL: 'http://localhost:3000',

  // API endpoints
  ENDPOINTS: {
    CONVERSATIONS: '/api/conversations',
    MESSAGES: (conversationId) => `/api/conversations/${conversationId}/messages`,
  }
};

// Export for use in other scripts
window.API_CONFIG = API_CONFIG;
