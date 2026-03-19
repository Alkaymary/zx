// ---- Auth Helper ----
// Manages JWT tokens (accessToken, refreshToken) in sessionStorage

const Auth = {
    storageKeys: ['accessToken', 'refreshToken', 'expiresAt', 'userId', 'username', 'roleCode'],

    getStorage() {
        return window.sessionStorage;
    },

    getLegacyStorage() {
        return window.localStorage;
    },

    setStoredValue(key, value) {
        if (value === undefined || value === null) return;

        this.getStorage().setItem(key, String(value));
        this.getLegacyStorage().removeItem(key);
    },

    getStoredValue(key) {
        const sessionValue = this.getStorage().getItem(key);
        if (sessionValue !== null) {
            return sessionValue;
        }

        const legacyValue = this.getLegacyStorage().getItem(key);
        if (legacyValue !== null) {
            this.getStorage().setItem(key, legacyValue);
            this.getLegacyStorage().removeItem(key);
        }

        return legacyValue;
    },

    removeStoredValue(key) {
        this.getStorage().removeItem(key);
        this.getLegacyStorage().removeItem(key);
    },

    clearStoredAuth() {
        this.storageKeys.forEach((key) => this.removeStoredValue(key));
    },

    // Save tokens after login
    saveTokens(data) {
        this.setStoredValue('accessToken', data.accessToken);
        this.setStoredValue('refreshToken', data.refreshToken);
        this.setStoredValue('expiresAt', data.expiresAt);
        this.setStoredValue('userId', data.userId);
        this.setStoredValue('username', data.username);
        this.setStoredValue('roleCode', data.roleCode || '');
    },

    // Get access token
    getAccessToken() {
        return this.getStoredValue('accessToken');
    },

    getRefreshToken() {
        return this.getStoredValue('refreshToken');
    },

    getUsername() {
        return this.getStoredValue('username') || 'مدير النظام';
    },

    getRoleCode() {
        return this.getStoredValue('roleCode') || '';
    },

    getDisplayName() {
        const roleCode = this.getRoleCode();

        switch (roleCode) {
            case 'super_admin':
                return 'مدير عام';
            case 'admin':
                return 'مدير النظام';
            case 'finance':
                return 'المحاسب';
            case 'support':
                return 'الدعم الفني';
            case 'viewer':
                return 'العرض';
            default:
                return 'مستخدم';
        }
    },

    // Check if logged in
    isLoggedIn() {
        return !!this.getAccessToken();
    },

    // Clear tokens (logout)
    logout() {
        this.clearStoredAuth();
        window.location.href = 'index.html';
    },

    // Redirect to login if not authenticated
    requireAuth() {
        if (!this.isLoggedIn()) {
            window.location.href = 'index.html';
            return false;
        }
        return true;
    },

    // Refresh the access token using refresh token
    async refreshAccessToken() {
        const refreshToken = this.getRefreshToken();
        if (!refreshToken) {
            this.logout();
            return null;
        }

        try {
            const res = await fetch('/api/auth/refresh', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken })
            });

            if (!res.ok) {
                this.logout();
                return null;
            }

            const data = await res.json();
            this.saveTokens(data);
            return data.accessToken;
        } catch {
            this.logout();
            return null;
        }
    },

    // Make authenticated fetch call — auto-refreshes token on 401
    async apiFetch(url, options = {}) {
        let token = this.getAccessToken();
        if (!token) {
            this.logout();
            return null;
        }

        options.headers = {
            ...options.headers,
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        };

        let res = await fetch(url, options);

        // If 401, try refreshing the token once
        if (res.status === 401) {
            token = await this.refreshAccessToken();
            if (!token) return null;

            options.headers['Authorization'] = `Bearer ${token}`;
            res = await fetch(url, options);
        }

        return res;
    }
};

window.Auth = Auth;
