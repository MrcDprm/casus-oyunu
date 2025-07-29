// Toast Notification System
class ToastManager {
    constructor() {
        this.container = this.createContainer();
        this.toasts = [];
    }

    createContainer() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            max-width: 350px;
        `;
        document.body.appendChild(container);
        return container;
    }

    show(message, type = 'info', duration = 5000) {
        const toast = this.createToast(message, type);
        this.container.appendChild(toast);
        this.toasts.push(toast);

        // Animate in
        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
            toast.style.opacity = '1';
        }, 100);

        // Auto remove
        if (duration > 0) {
            setTimeout(() => {
                this.remove(toast);
            }, duration);
        }

        return toast;
    }

    createToast(message, type) {
        const toast = document.createElement('div');
        const icon = this.getIcon(type);
        const color = this.getColor(type);

        toast.style.cssText = `
            background: white;
            border-left: 4px solid ${color};
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            margin-bottom: 10px;
            padding: 16px;
            transform: translateX(100%);
            opacity: 0;
            transition: all 0.3s ease;
            display: flex;
            align-items: center;
            min-width: 300px;
        `;

        toast.innerHTML = `
            <div style="margin-right: 12px; font-size: 18px; color: ${color};">
                ${icon}
            </div>
            <div style="flex: 1;">
                <div style="font-weight: 500; margin-bottom: 4px;">
                    ${this.getTitle(type)}
                </div>
                <div style="color: #666; font-size: 14px;">
                    ${message}
                </div>
            </div>
            <button onclick="toastManager.remove(this.parentElement)" 
                    style="background: none; border: none; color: #999; cursor: pointer; padding: 4px; margin-left: 8px;">
                <i class="fas fa-times"></i>
            </button>
        `;

        return toast;
    }

    remove(toast) {
        if (toast && toast.parentElement) {
            toast.style.transform = 'translateX(100%)';
            toast.style.opacity = '0';
            
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.parentElement.removeChild(toast);
                }
                this.toasts = this.toasts.filter(t => t !== toast);
            }, 300);
        }
    }

    getIcon(type) {
        const icons = {
            success: '<i class="fas fa-check-circle"></i>',
            error: '<i class="fas fa-exclamation-circle"></i>',
            warning: '<i class="fas fa-exclamation-triangle"></i>',
            info: '<i class="fas fa-info-circle"></i>'
        };
        return icons[type] || icons.info;
    }

    getColor(type) {
        const colors = {
            success: '#28a745',
            error: '#dc3545',
            warning: '#ffc107',
            info: '#17a2b8'
        };
        return colors[type] || colors.info;
    }

    getTitle(type) {
        const titles = {
            success: 'Başarılı',
            error: 'Hata',
            warning: 'Uyarı',
            info: 'Bilgi'
        };
        return titles[type] || titles.info;
    }

    // Convenience methods
    success(message, duration = 5000) {
        return this.show(message, 'success', duration);
    }

    error(message, duration = 5000) {
        return this.show(message, 'error', duration);
    }

    warning(message, duration = 5000) {
        return this.show(message, 'warning', duration);
    }

    info(message, duration = 5000) {
        return this.show(message, 'info', duration);
    }

    // Clear all toasts
    clear() {
        this.toasts.forEach(toast => this.remove(toast));
    }
}

// Global toast manager instance
const toastManager = new ToastManager();

// Auto-show toasts from TempData
document.addEventListener('DOMContentLoaded', function() {
    // Check for TempData messages
    const successMessage = document.querySelector('[data-toast="success"]');
    const errorMessage = document.querySelector('[data-toast="error"]');
    const warningMessage = document.querySelector('[data-toast="warning"]');
    const infoMessage = document.querySelector('[data-toast="info"]');

    if (successMessage) {
        toastManager.success(successMessage.textContent);
        successMessage.remove();
    }

    if (errorMessage) {
        toastManager.error(errorMessage.textContent);
        errorMessage.remove();
    }

    if (warningMessage) {
        toastManager.warning(warningMessage.textContent);
        warningMessage.remove();
    }

    if (infoMessage) {
        toastManager.info(infoMessage.textContent);
        infoMessage.remove();
    }
});

// Export for use in other scripts
window.toastManager = toastManager; 