export interface ConversationResponse {
  conversationId: string;
  partner: {
    id: string,
    displayName: string,           
    avatar: string,      
  };
  lastMessage: {
    lastContent: string,        
    timestamp: string,      
  };
  unreadCount: number;    
  updatedAt: Date;          
}