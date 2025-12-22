// --- SOCKET.IO REALTIME ---
let socket;

function setupSocket() {
    socket = io();
    // Khi chọn conversation thì join room
    document.addEventListener('selectConversation', (e) => {
        if (e.detail && e.detail.conversationId) {
            socket.emit('join_conversation', e.detail.conversationId);
        }
    });
    // Lắng nghe tin nhắn realtime
    socket.on('receive_message', (message) => {
        // Ensure messages is an array before pushing
        if (!Array.isArray(state.messages)) {
            state.messages = [];
        }
        state.messages.push(message);
        renderMessage(message);
    });
}

window.addEventListener('DOMContentLoaded', setupSocket);
// Chat Application - Client Side JavaScript

// State management
const state = {
    currentUserId: null,
    currentConversationId: null,
    conversations: [],
    messages: [],
    users: [],
};

// DOM Elements
const elements = {
    conversationList: document.getElementById('conversation-list'),
    messagesContainer: document.getElementById('messages-container'),
    messageForm: document.getElementById('message-form'),
    messageInput: document.getElementById('message-input'),
    sendBtn: document.getElementById('send-btn'),
    conversationName: document.getElementById('conversation-name'),
    conversationType: document.getElementById('conversation-type'),
    conversationAvatar: document.getElementById('conversation-avatar'),
    currentUserName: document.getElementById('current-user-name'),
    newConversationBtn: document.getElementById('new-conversation-btn'),
    newConversationModal: document.getElementById('new-conversation-modal'),
    closeModalBtn: document.getElementById('close-modal-btn'),
    cancelModalBtn: document.getElementById('cancel-modal-btn'),
    createConversationBtn: document.getElementById('create-conversation-btn'),
    conversationTypeSelect: document.getElementById('conversation-type-select'),
    groupNameGroup: document.getElementById('group-name-group'),
    groupNameInput: document.getElementById('group-name-input'),
    usersList: document.getElementById('users-list'),
    conversationInfoBtn: document.getElementById('conversation-info-btn'),
    infoSidebar: document.getElementById('info-sidebar'),
    closeInfoBtn: document.getElementById('close-info-btn'),
    infoContent: document.getElementById('info-content'),
};

// Utility functions
function formatTime(timestamp) {
    const date = new Date(timestamp);
    const now = new Date();
    const diff = now - date;
    
    // Nếu trong ngày hôm nay
    if (diff < 86400000) {
        return date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }
    
    // Nếu trong tuần này
    if (diff < 604800000) {
        return date.toLocaleDateString('vi-VN', { weekday: 'short' });
    }
    
    // Ngày tháng
    return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
}

function getInitials(name) {
    if (!name) return '?';
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
}


// Gán màu cố định cho mỗi user dựa trên userId (hoặc username)
const userColors = {};
const colorPalette = [
    'linear-gradient(135deg, #667eea, #764ba2)',
    'linear-gradient(135deg, #f093fb, #f5576c)',
    'linear-gradient(135deg, #4facfe, #00f2fe)',
    'linear-gradient(135deg, #43e97b, #38f9d7)',
    'linear-gradient(135deg, #fa709a, #fee140)',
];
function getUserColor(userId) {
    if (!userId) return colorPalette[0];
    if (userColors[userId]) return userColors[userId];
    // Hash userId để chọn màu cố định
    let hash = 0;
    for (let i = 0; i < userId.length; i++) {
        hash = userId.charCodeAt(i) + ((hash << 5) - hash);
    }
    const idx = Math.abs(hash) % colorPalette.length;
    userColors[userId] = colorPalette[idx];
    return userColors[userId];
}

// API calls
async function getCurrentUser() {
    try {
        // Get user from localStorage
        const userId = localStorage.getItem('userId');
        const username = localStorage.getItem('username');
        
        // Redirect to login if not logged in
        if (!userId || !username) {
            window.location.href = '/login';
            return;
        }
        
        state.currentUserId = userId;
        elements.currentUserName.textContent = username;
        
        return userId;
    } catch (error) {
        console.error('Error getting current user:', error);
        window.location.href = '/login';
    }
}

async function loadConversations() {
    try {
        const response = await fetch('/api/conversations', {
            headers: {
                'x-user-id': state.currentUserId,
                'x-username': localStorage.getItem('username') || 'Unknown',
            },
        });

        const result = await response.json();

        if (!result.success) {
            const error = new Error(result.error?.message || 'Failed to load conversations');
            error.correlationId = result.correlationId;
            throw error;
        }

        const conversations = result.data || [];
        state.conversations = conversations;
        renderConversations(conversations);
    } catch (error) {
        console.error('Error loading conversations:', error);
        showError(
            error.message || 'Không thể tải danh sách conversations',
            error.correlationId
        );
    }
}

async function loadMessages(conversationId) {
    try {
        const response = await fetch(`/api/conversations/${conversationId}/messages`, {
            headers: {
                'x-user-id': state.currentUserId,
            },
        });

        const result = await response.json();

        if (!result.success) {
            const error = new Error(result.error?.message || 'Failed to load messages');
            error.correlationId = result.correlationId;
            throw error;
        }

        // Backend returns { success: true, data: { data: [...], pagination: {...} } }
        const messages = result.data?.data || [];

        // Ensure messages is always an array
        if (!Array.isArray(messages)) {
            console.error('Messages is not an array:', messages);
            state.messages = [];
        } else {
            state.messages = messages;
        }

        renderMessages(state.messages);
    } catch (error) {
        console.error('Error loading messages:', error);
        showError(
            error.message || 'Không thể tải tin nhắn',
            error.correlationId
        );
    }
}

async function sendMessage(conversationId, content) {
    try {
        const response = await fetch(
            `/api/conversations/${conversationId}/messages`,
            {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'x-user-id': state.currentUserId,
                    'x-username': localStorage.getItem('username') || 'Unknown',
                },
                body: JSON.stringify({ content }),
            }
        );

        const result = await response.json();

        if (!result.success) {
            const error = new Error(result.error?.message || 'Failed to send message');
            error.correlationId = result.correlationId;
            throw error;
        }

        // Không render ngay, chỉ render khi nhận qua socket để tránh duplicate

        // Scroll to bottom
        elements.messagesContainer.scrollTop =
            elements.messagesContainer.scrollHeight;

        // Không return gì cả vì không còn biến message
    } catch (error) {
        console.error('Error sending message:', error);
        showError(
            error.message || 'Không thể gửi tin nhắn',
            error.correlationId
        );
    }
}

async function loadUsers() {
    try {
        // TODO: Implement API endpoint to get all users
        // const response = await fetch('/api/users');
        // if (!response.ok) throw new Error('Failed to load users');
        // const users = await response.json();
        
        // Mock data with real ObjectIds from seeded database
        const users = [
            { _id: '693d82480025a5c1d8f5b33f', username: 'Bob' },
            { _id: '693d82480025a5c1d8f5b340', username: 'Charlie' },
            { _id: '693d82480025a5c1d8f5b341', username: 'Diana' },
        ].filter(u => u._id !== state.currentUserId);
        
        state.users = users;
        return users;
    } catch (error) {
        console.error('Error loading users:', error);
        return [];
    }
}

async function createConversation(type, participantIds, name = null) {
    try {
        // Map participantIds to participantsReq format
        const participantsReq = participantIds.map(userId => {
            const user = state.users.find(u => u._id === userId);
            return {
                id: userId,
                username: user ? user.username : 'Unknown',
                avatarUrl: null
            };
        });

        const response = await fetch('/api/conversations', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'x-user-id': state.currentUserId,
                'x-username': localStorage.getItem('username') || 'Unknown',
            },
            body: JSON.stringify({ type, participantsReq, name }),
        });

        const result = await response.json();

        if (!result.success) {
            const error = new Error(result.error?.message || 'Failed to create conversation');
            error.correlationId = result.correlationId;
            throw error;
        }

        return result.data;
    } catch (error) {
        console.error('Error creating conversation:', error);
        showError(
            error.message || 'Không thể tạo conversation',
            error.correlationId
        );
    }
}

// Render functions
function renderConversations(conversations) {
    elements.conversationList.innerHTML = '';
    
    if (conversations.length === 0) {
        elements.conversationList.innerHTML = `
            <div style="padding: 20px; text-align: center; color: var(--text-secondary);">
                Chưa có conversation nào
            </div>
        `;
        return;
    }
    
    conversations.forEach(conversation => {
        const item = document.createElement('div');
        item.className = 'conversation-item';
        item.dataset.id = conversation._id;
        
        const avatar = document.createElement('div');
        avatar.className = 'conversation-avatar';
        avatar.style.background = getUserColor(conversation._id);
        avatar.textContent = getInitials(conversation.name);
        
        const info = document.createElement('div');
        info.className = 'conversation-info';
        
        const header = document.createElement('div');
        header.className = 'conversation-header';
        
        const name = document.createElement('div');
        name.className = 'conversation-item-name';
        name.textContent = conversation.name || 'Conversation';
        
        const time = document.createElement('div');
        time.className = 'conversation-time';
        time.textContent = formatTime(conversation.updatedAt || conversation.createdAt);
        
        header.appendChild(name);
        header.appendChild(time);
        
        const preview = document.createElement('div');
        preview.className = 'conversation-preview';
        preview.textContent = 'Nhấn để xem tin nhắn...';
        
        info.appendChild(header);
        info.appendChild(preview);
        
        item.appendChild(avatar);
        item.appendChild(info);
        
        item.addEventListener('click', () => selectConversation(conversation));
        
        elements.conversationList.appendChild(item);
    });
}

function renderMessages(messages) {
    elements.messagesContainer.innerHTML = '';
    
    if (messages.length === 0) {
        elements.messagesContainer.innerHTML = `
            <div class="welcome-message">
                <h2>Bắt đầu cuộc trò chuyện</h2>
                <p>Gửi tin nhắn đầu tiên của bạn</p>
            </div>
        `;
        return;
    }
    
    messages.forEach(message => renderMessage(message));
}

function renderMessage(message) {
    const messageEl = document.createElement('div');
    messageEl.className = 'message';
    messageEl.className += message.sender.id === state.currentUserId 
        ? ' sent' 
        : ' received';
    
    const avatar = document.createElement('div');
    avatar.className = 'message-avatar';
    avatar.style.background = getUserColor(message.sender.id);
    avatar.textContent = getInitials(message.sender.name);
    
    const content = document.createElement('div');
    content.className = 'message-content';
    
    // Show sender name for group chats
    if (message.sender.id !== state.currentUserId) {
        const sender = document.createElement('div');
        sender.className = 'message-sender';
        sender.textContent = message.sender.name;
        content.appendChild(sender);
    }
    
    const bubble = document.createElement('div');
    bubble.className = 'message-bubble';
    bubble.textContent = message.content;
    
    const time = document.createElement('div');
    time.className = 'message-time';
    time.textContent = formatTime(message.timestamp);
    
    content.appendChild(bubble);
    content.appendChild(time);
    
    messageEl.appendChild(avatar);
    messageEl.appendChild(content);
    
    if (elements.messagesContainer.querySelector('.welcome-message')) {
        elements.messagesContainer.innerHTML = '';
    }
    
    elements.messagesContainer.appendChild(messageEl);
}

function renderUsersList(users) {
    elements.usersList.innerHTML = '';
    
    users.forEach(user => {
        const item = document.createElement('div');
        item.className = 'user-item';
        
        const checkbox = document.createElement('input');
        checkbox.type = 'checkbox';
        checkbox.value = user._id;
        checkbox.id = `user-${user._id}`;
        
        const avatar = document.createElement('div');
        avatar.className = 'user-avatar';
        avatar.style.width = '32px';
        avatar.style.height = '32px';
        avatar.style.fontSize = '12px';
        avatar.style.background = getUserColor(user._id);
        avatar.textContent = getInitials(user.username);
        
        const name = document.createElement('label');
        name.textContent = user.username;
        name.htmlFor = `user-${user._id}`;
        name.style.cursor = 'pointer';
        name.style.flex = '1';
        
        item.appendChild(checkbox);
        item.appendChild(avatar);
        item.appendChild(name);
        
        elements.usersList.appendChild(item);
    });
}

// Event handlers
function selectConversation(conversation) {
    state.currentConversationId = conversation._id;
    // Phát sự kiện để socket join room
    const event = new CustomEvent('selectConversation', { detail: { conversationId: conversation._id } });
    document.dispatchEvent(event);
    
    // Update active state
    document.querySelectorAll('.conversation-item').forEach(item => {
        item.classList.remove('active');
    });
    document.querySelector(
        `.conversation-item[data-id="${conversation._id}"]`
    )?.classList.add('active');
    
    // Update header
    elements.conversationName.textContent = conversation.name || 'Conversation';
    elements.conversationType.textContent = 
        conversation.type === 'GROUP' ? 'Nhóm' : 'Trò chuyện riêng';
    elements.conversationAvatar.textContent = getInitials(conversation.name);
    elements.conversationAvatar.style.background = getUserColor(conversation._id);
    
    // Enable input
    elements.messageInput.disabled = false;
    elements.sendBtn.disabled = false;
    elements.messageInput.focus();
    
    // Load messages
    loadMessages(conversation._id);
}

function handleSendMessage(e) {
    e.preventDefault();
    
    const content = elements.messageInput.value.trim();
    if (!content || !state.currentConversationId) return;
    
    sendMessage(state.currentConversationId, content);
    elements.messageInput.value = '';
}

function showError(message, correlationId) {
    // Simple alert for now, could be replaced with a toast notification
    let errorMessage = message;
    if (correlationId) {
        errorMessage += `\n\nCorrelation ID: ${correlationId}`;
        console.error(`[${correlationId}] ${message}`);
    }
    alert(errorMessage);
}

function openNewConversationModal() {
    elements.newConversationModal.classList.add('show');
    loadUsers().then(users => renderUsersList(users));
}

function closeNewConversationModal() {
    elements.newConversationModal.classList.remove('show');
    elements.conversationTypeSelect.value = 'SINGLE';
    elements.groupNameGroup.style.display = 'none';
    elements.groupNameInput.value = '';
    
    // Uncheck all users
    document.querySelectorAll('#users-list input[type="checkbox"]')
        .forEach(cb => cb.checked = false);
}

function handleCreateConversation() {
    const type = elements.conversationTypeSelect.value;
    const selectedUsers = Array.from(
        document.querySelectorAll('#users-list input[type="checkbox"]:checked')
    ).map(cb => cb.value);
    
    if (selectedUsers.length === 0) {
        showError('Vui lòng chọn ít nhất một người');
        return;
    }
    
    if (type === 'SINGLE' && selectedUsers.length !== 1) {
        showError('Trò chuyện riêng chỉ có thể chọn một người');
        return;
    }
    
    const name = type === 'GROUP' ? elements.groupNameInput.value.trim() : null;
    
    if (type === 'GROUP' && !name) {
        showError('Vui lòng nhập tên nhóm');
        return;
    }
    
    createConversation(type, selectedUsers, name).then(() => {
        closeNewConversationModal();
        loadConversations();
    });
}

function toggleInfoSidebar() {
    elements.infoSidebar.classList.toggle('show');
    
    if (elements.infoSidebar.classList.contains('show')) {
        loadConversationInfo();
    }
}

function loadConversationInfo() {
    if (!state.currentConversationId) return;
    
    const conversation = state.conversations.find(
        c => c._id === state.currentConversationId
    );
    
    if (!conversation) return;
    
    elements.infoContent.innerHTML = `
        <div class="info-section">
            <h4>Thông tin conversation</h4>
            <div style="margin-bottom: 8px;">
                <strong>Tên:</strong> ${conversation.name || 'N/A'}
            </div>
            <div style="margin-bottom: 8px;">
                <strong>Loại:</strong> ${conversation.type === 'GROUP' ? 'Nhóm' : 'Riêng tư'}
            </div>
            <div>
                <strong>Tạo lúc:</strong> ${new Date(conversation.createdAt).toLocaleString('vi-VN')}
            </div>
        </div>
        
        <div class="info-section">
            <h4>Thành viên</h4>
            <div style="color: var(--text-secondary); font-size: 14px;">
                Đang tải...
            </div>
        </div>
    `;
}

// Event listeners
elements.messageForm.addEventListener('submit', handleSendMessage);
elements.newConversationBtn.addEventListener('click', openNewConversationModal);
elements.closeModalBtn.addEventListener('click', closeNewConversationModal);
elements.cancelModalBtn.addEventListener('click', closeNewConversationModal);
elements.createConversationBtn.addEventListener('click', handleCreateConversation);
elements.conversationInfoBtn.addEventListener('click', toggleInfoSidebar);
elements.closeInfoBtn.addEventListener('click', () => {
    elements.infoSidebar.classList.remove('show');
});

elements.conversationTypeSelect.addEventListener('change', (e) => {
    if (e.target.value === 'GROUP') {
        elements.groupNameGroup.style.display = 'block';
    } else {
        elements.groupNameGroup.style.display = 'none';
    }
});

// Initialize app
async function init() {
    await getCurrentUser();
    await loadConversations();
}

// Logout function
function logout() {
    if (confirm('Bạn có chắc muốn đăng xuất?')) {
        localStorage.removeItem('userId');
        localStorage.removeItem('username');
        window.location.href = '/login';
    }
}

// Start the app
init();
