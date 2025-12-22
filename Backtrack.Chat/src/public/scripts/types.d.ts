/**
 * API Response Type Definitions
 * Matches backend response format from RESPONSE_FORMAT.md
 */

/**
 * Standard API Response wrapper
 * @template T - The type of data returned on success
 */
interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: ApiError;
  correlationId?: string;
}

/**
 * API Error structure
 */
interface ApiError {
  code: string;
  message: string;
  details?: unknown;
}

/**
 * Conversation participant
 */
interface Participant {
  id: string;
  name: string;
  avatarUrl?: string;
}

/**
 * Conversation type
 */
type ConversationType = 'SINGLE' | 'GROUP';

/**
 * Conversation object
 */
interface Conversation {
  _id: string;
  name: string;
  type: ConversationType;
  participants: Participant[];
  createdAt: string;
  updatedAt: string;
}

/**
 * Message sender info
 */
interface MessageSender {
  id: string;
  name: string;
  avatarUrl?: string;
}

/**
 * Message object
 */
interface Message {
  _id: string;
  conversationId: string;
  sender: MessageSender;
  content: string;
  timestamp: string;
  createdAt: string;
  updatedAt: string;
}

/**
 * Cursor-based pagination info (used by message endpoints)
 */
interface CursorPagination {
  nextCursor?: string;
  hasMore: boolean;
}

/**
 * Page-based pagination info (used by other endpoints)
 */
interface PagePagination {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
}

/**
 * Cursor-based paginated response wrapper (messages)
 */
interface CursorPaginatedResponse<T> {
  data: T[];
  pagination: CursorPagination;
}

/**
 * Page-based paginated response wrapper
 */
interface PagePaginatedResponse<T> {
  items: T[];
  pagination: PagePagination;
}

/**
 * User object
 */
interface User {
  _id: string;
  username: string;
  avatarUrl?: string;
}

/**
 * Create conversation request
 */
interface CreateConversationRequest {
  type: ConversationType;
  participantsReq: Array<{
    id: string;
    username: string;
    avatarUrl?: string | null;
  }>;
  name?: string | null;
}

/**
 * Send message request
 */
interface SendMessageRequest {
  content: string;
}

/**
 * Extended Error with correlation ID
 */
interface ExtendedError extends Error {
  code?: string;
  correlationId?: string;
  statusCode?: number;
}
