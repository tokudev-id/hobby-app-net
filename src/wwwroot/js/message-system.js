/**
 * Global Message System for Success/Error notifications
 */

class MessageSystem {
    constructor() {
        this.container = null;
        this.init();
    }

    init() {
        // Create message container if it doesn't exist
        if (!document.getElementById('messageContainer')) {
            this.container = document.createElement('div');
            this.container.id = 'messageContainer';
            this.container.className = 'message-container';
            document.body.appendChild(this.container);
        } else {
            this.container = document.getElementById('messageContainer');
        }
    }

    show(message, type = 'info', duration = 5000) {
        const messageElement = document.createElement('div');
        messageElement.className = `message message-${type}`;
        
        // Add icon based on type
        let icon = '';
        switch (type) {
            case 'success':
                icon = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 12l2 2 4-4"></path><circle cx="12" cy="12" r="9"></circle></svg>';
                break;
            case 'error':
                icon = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>';
                break;
            case 'warning':
                icon = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>';
                break;
            case 'info':
            default:
                icon = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>';
                break;
        }

        messageElement.innerHTML = `
            <div class="message-icon">${icon}</div>
            <div class="message-content">${message}</div>
            <button class="message-close" onclick="messageSystem.close(this.parentElement)">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"></line>
                    <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
            </button>
        `;

        // Add animation class
        messageElement.style.opacity = '0';
        messageElement.style.transform = 'translateX(100%)';
        
        this.container.appendChild(messageElement);

        // Animate in
        setTimeout(() => {
            messageElement.style.opacity = '1';
            messageElement.style.transform = 'translateX(0)';
        }, 10);

        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => {
                this.close(messageElement);
            }, duration);
        }

        return messageElement;
    }

    close(messageElement) {
        if (messageElement && messageElement.parentNode) {
            // Animate out
            messageElement.style.opacity = '0';
            messageElement.style.transform = 'translateX(100%)';
            
            setTimeout(() => {
                if (messageElement.parentNode) {
                    messageElement.parentNode.removeChild(messageElement);
                }
            }, 300);
        }
    }

    success(message, duration = 5000) {
        return this.show(message, 'success', duration);
    }

    error(message, duration = 7000) {
        return this.show(message, 'error', duration);
    }

    warning(message, duration = 6000) {
        return this.show(message, 'warning', duration);
    }

    info(message, duration = 5000) {
        return this.show(message, 'info', duration);
    }

    clear() {
        if (this.container) {
            this.container.innerHTML = '';
        }
    }
}

// Create global instance
window.messageSystem = new MessageSystem();

// Add CSS styles
const messageStyles = `
    .message-container {
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        max-width: 400px;
        pointer-events: none;
    }

    .message {
        display: flex;
        align-items: center;
        gap: 12px;
        margin-bottom: 12px;
        padding: 16px;
        border-radius: 12px;
        color: white;
        font-weight: 500;
        box-shadow: 0 8px 32px rgba(0, 0, 0, 0.12);
        backdrop-filter: blur(10px);
        border: 1px solid rgba(255, 255, 255, 0.1);
        pointer-events: auto;
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
        position: relative;
        overflow: hidden;
    }

    .message::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        height: 3px;
        background: rgba(255, 255, 255, 0.3);
    }

    .message-success {
        background: linear-gradient(135deg, #10b981, #059669);
    }

    .message-error {
        background: linear-gradient(135deg, #ef4444, #dc2626);
    }

    .message-warning {
        background: linear-gradient(135deg, #f59e0b, #d97706);
    }

    .message-info {
        background: linear-gradient(135deg, #3b82f6, #2563eb);
    }

    .message-icon {
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .message-content {
        flex: 1;
        font-size: 14px;
        line-height: 1.4;
    }

    .message-close {
        flex-shrink: 0;
        background: none;
        border: none;
        color: rgba(255, 255, 255, 0.8);
        cursor: pointer;
        padding: 4px;
        border-radius: 6px;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s ease;
    }

    .message-close:hover {
        background: rgba(255, 255, 255, 0.2);
        color: white;
    }

    @media (max-width: 640px) {
        .message-container {
            left: 20px;
            right: 20px;
            max-width: none;
        }

        .message {
            margin-bottom: 8px;
            padding: 12px;
        }

        .message-content {
            font-size: 13px;
        }
    }
`;

// Inject styles
const styleSheet = document.createElement('style');
styleSheet.textContent = messageStyles;
document.head.appendChild(styleSheet);
