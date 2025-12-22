// Login functionality
const elements = {
    form: document.getElementById('login-form'),
    usernameInput: document.getElementById('username'),
    btnLogin: document.getElementById('btn-login'),
    errorMessage: document.getElementById('error-message'),
    userTags: document.getElementById('user-tags'),
};

// Available users (hardcoded for demo)
const availableUsers = [
    { username: 'Alice', id: '693d82480025a5c1d8f5b33e' },
    { username: 'Bob', id: '693d82480025a5c1d8f5b33f' },
    { username: 'Charlie', id: '693d82480025a5c1d8f5b340' },
    { username: 'Diana', id: '693d82480025a5c1d8f5b341' },
];

// Show error message
function showError(message) {
    elements.errorMessage.textContent = message;
    elements.errorMessage.classList.add('show');
    setTimeout(() => {
        elements.errorMessage.classList.remove('show');
    }, 3000);
}

// Login with username
async function login(username) {
    try {
        elements.btnLogin.disabled = true;
        elements.btnLogin.textContent = 'Đang đăng nhập...';

        // Find user by username (case insensitive)
        const user = availableUsers.find(
            u => u.username.toLowerCase() === username.toLowerCase()
        );

        if (!user) {
            showError(`Không tìm thấy user "${username}". Vui lòng chọn user có sẵn.`);
            elements.btnLogin.disabled = false;
            elements.btnLogin.textContent = 'Đăng nhập';
            return;
        }

        // Save to localStorage
        localStorage.setItem('userId', user.id);
        localStorage.setItem('username', user.username);

        // Redirect to chat
        window.location.href = '/chat';
    } catch (error) {
        console.error('Login error:', error);
        showError('Có lỗi xảy ra. Vui lòng thử lại.');
        elements.btnLogin.disabled = false;
        elements.btnLogin.textContent = 'Đăng nhập';
    }
}

// Handle form submit
elements.form.addEventListener('submit', (e) => {
    e.preventDefault();
    const username = elements.usernameInput.value.trim();
    
    if (!username) {
        showError('Vui lòng nhập tên');
        return;
    }

    login(username);
});

// Render user tags
function renderUserTags() {
    availableUsers.forEach(user => {
        const tag = document.createElement('span');
        tag.className = 'user-tag';
        tag.textContent = user.username;
        tag.addEventListener('click', () => {
            elements.usernameInput.value = user.username;
            elements.usernameInput.focus();
        });
        elements.userTags.appendChild(tag);
    });
}

// Initialize
renderUserTags();

// Auto-fill if already logged in (for testing)
const savedUsername = localStorage.getItem('username');
if (savedUsername) {
    elements.usernameInput.value = savedUsername;
}
