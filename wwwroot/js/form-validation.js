// Advanced Form Validation
class FormValidator {
    constructor(formSelector, options = {}) {
        this.form = document.querySelector(formSelector);
        this.options = {
            showToast: true,
            validateOnInput: true,
            validateOnBlur: true,
            ...options
        };
        
        if (this.form) {
            this.init();
        }
    }

    init() {
        this.setupEventListeners();
        this.setupCustomValidators();
    }

    setupEventListeners() {
        if (this.options.validateOnInput) {
            this.form.addEventListener('input', (e) => {
                this.validateField(e.target);
            });
        }

        if (this.options.validateOnBlur) {
            this.form.addEventListener('blur', (e) => {
                this.validateField(e.target);
            }, true);
        }

        this.form.addEventListener('submit', (e) => {
            if (!this.validateForm()) {
                e.preventDefault();
                if (this.options.showToast) {
                    toastManager.error('Lütfen form hatalarını düzeltin.');
                }
            }
        });
    }

    setupCustomValidators() {
        // Custom validators
        this.validators = {
            required: (value) => value.trim() !== '' || 'Bu alan zorunludur.',
            email: (value) => {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                return emailRegex.test(value) || 'Geçerli bir e-posta adresi girin.';
            },
            minLength: (value, min) => {
                return value.length >= min || `En az ${min} karakter olmalıdır.`;
            },
            maxLength: (value, max) => {
                return value.length <= max || `En fazla ${max} karakter olmalıdır.`;
            },
            pattern: (value, pattern) => {
                const regex = new RegExp(pattern);
                return regex.test(value) || 'Geçersiz format.';
            },
            roomCode: (value) => {
                const roomCodeRegex = /^[A-Z0-9]{6}$/;
                return roomCodeRegex.test(value.toUpperCase()) || 'Oda kodu 6 karakter olmalıdır (sadece harf ve rakam).';
            },
            username: (value) => {
                const usernameRegex = /^[a-zA-Z0-9_]{3,20}$/;
                return usernameRegex.test(value) || 'Kullanıcı adı 3-20 karakter olmalıdır (sadece harf, rakam ve alt çizgi).';
            },
            password: (value) => {
                const hasLetter = /[a-zA-Z]/.test(value);
                const hasNumber = /\d/.test(value);
                const hasMinLength = value.length >= 6;
                
                if (!hasMinLength) return 'Şifre en az 6 karakter olmalıdır.';
                if (!hasLetter) return 'Şifre en az bir harf içermelidir.';
                if (!hasNumber) return 'Şifre en az bir rakam içermelidir.';
                
                return true;
            }
        };
    }

    validateField(field) {
        const rules = this.getFieldRules(field);
        let isValid = true;
        let errorMessage = '';

        // Clear previous error
        this.clearFieldError(field);

        // Check each rule
        for (const rule of rules) {
            const result = this.validators[rule.type](field.value, rule.value);
            
            if (result !== true) {
                isValid = false;
                errorMessage = result;
                break;
            }
        }

        // Show/hide error
        if (!isValid) {
            this.showFieldError(field, errorMessage);
        } else {
            this.showFieldSuccess(field);
        }

        return isValid;
    }

    validateForm() {
        const fields = this.form.querySelectorAll('input, select, textarea');
        let isValid = true;

        fields.forEach(field => {
            if (!this.validateField(field)) {
                isValid = false;
            }
        });

        return isValid;
    }

    getFieldRules(field) {
        const rules = [];
        
        // Required rule
        if (field.hasAttribute('required')) {
            rules.push({ type: 'required' });
        }

        // Email rule
        if (field.type === 'email') {
            rules.push({ type: 'email' });
        }

        // Min length
        if (field.hasAttribute('minlength')) {
            rules.push({ 
                type: 'minLength', 
                value: parseInt(field.getAttribute('minlength')) 
            });
        }

        // Max length
        if (field.hasAttribute('maxlength')) {
            rules.push({ 
                type: 'maxLength', 
                value: parseInt(field.getAttribute('maxlength')) 
            });
        }

        // Pattern
        if (field.hasAttribute('pattern')) {
            rules.push({ 
                type: 'pattern', 
                value: field.getAttribute('pattern') 
            });
        }

        // Custom data attributes
        if (field.dataset.validator) {
            const customRules = field.dataset.validator.split(',');
            customRules.forEach(rule => {
                const [type, value] = rule.trim().split(':');
                rules.push({ type, value });
            });
        }

        return rules;
    }

    showFieldError(field, message) {
        // Add error class
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');

        // Create or update error message
        let errorElement = field.parentNode.querySelector('.invalid-feedback');
        if (!errorElement) {
            errorElement = document.createElement('div');
            errorElement.className = 'invalid-feedback';
            field.parentNode.appendChild(errorElement);
        }
        errorElement.textContent = message;

        // Add error icon
        this.addFieldIcon(field, 'error');
    }

    showFieldSuccess(field) {
        // Add success class
        field.classList.add('is-valid');
        field.classList.remove('is-invalid');

        // Remove error message
        const errorElement = field.parentNode.querySelector('.invalid-feedback');
        if (errorElement) {
            errorElement.remove();
        }

        // Add success icon
        this.addFieldIcon(field, 'success');
    }

    clearFieldError(field) {
        field.classList.remove('is-invalid', 'is-valid');
        
        const errorElement = field.parentNode.querySelector('.invalid-feedback');
        if (errorElement) {
            errorElement.remove();
        }

        this.removeFieldIcon(field);
    }

    addFieldIcon(field, type) {
        this.removeFieldIcon(field);

        const icon = document.createElement('i');
        icon.className = `fas fa-${type === 'success' ? 'check' : 'exclamation-triangle'} form-control-feedback`;
        icon.style.cssText = `
            position: absolute;
            right: 10px;
            top: 50%;
            transform: translateY(-50%);
            color: ${type === 'success' ? '#28a745' : '#dc3545'};
            z-index: 2;
        `;

        field.parentNode.style.position = 'relative';
        field.parentNode.appendChild(icon);
    }

    removeFieldIcon(field) {
        const icon = field.parentNode.querySelector('.form-control-feedback');
        if (icon) {
            icon.remove();
        }
    }

    // Public methods
    reset() {
        const fields = this.form.querySelectorAll('input, select, textarea');
        fields.forEach(field => {
            field.value = '';
            this.clearFieldError(field);
        });
    }

    isValid() {
        return this.validateForm();
    }
}

// Auto-initialize form validators
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all forms with validation
    const forms = document.querySelectorAll('form[data-validate]');
    forms.forEach(form => {
        new FormValidator(`#${form.id}`);
    });

    // Special handling for room code input
    const roomCodeInput = document.querySelector('#roomCode');
    if (roomCodeInput) {
        roomCodeInput.addEventListener('input', function(e) {
            this.value = this.value.toUpperCase();
        });
    }

    // Auto-focus first input
    const firstInput = document.querySelector('form input:not([type="hidden"]), form select, form textarea');
    if (firstInput) {
        firstInput.focus();
    }
});

// Export for use in other scripts
window.FormValidator = FormValidator; 