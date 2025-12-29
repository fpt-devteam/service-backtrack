export interface ConversationResponse {
  conversationId: string;
  partner: {
    id: string,
    displayName: string,           
    avatar: string,      
  };
  lastMessage: {
    senderId: string,
    lastContent: string,        
    timestamp: string,      
  };
  unreadCount: number;    
  updatedAt: Date;          
}