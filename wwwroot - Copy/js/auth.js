// ---- Auth Helper ----
// Manages admin JWT tokens, role-aware page access, and legacy session compatibility.

const Auth = {
    storageKeys: ['accessToken', 'refreshToken', 'expiresAt', 'userId', 'username', 'roleCode'],

    areaLabels: {
        dashboard: 'لوحة التحكم',
        search: 'البحث العام',
        offices: 'المكاتب',
        devices: 'أجهزة POS',
        finance: 'الملفات المالية',
        users: 'المستخدمون',
        roles: 'أدوار الأدمن',
        reports: 'سجلات النظام',
        performance: 'الأداء المالي'
    },

    roleProfiles: {
        general_manager: {
            displayName: 'المدير العام',
            description: 'وصول كامل إلى جميع أقسام لوحة التحكم.',
            permissions: {
                dashboard: 'manage',
                search: 'manage',
                offices: 'manage',
                devices: 'manage',
                finance: 'manage',
                users: 'manage',
                roles: 'manage',
                reports: 'manage',
                performance: 'manage'
            }
        },
        accountant: {
            displayName: 'المحاسب',
            description: 'إدارة المكاتب والعمليات المالية مع عرض الأجهزة فقط.',
            permissions: {
                dashboard: 'read',
                offices: 'manage',
                devices: 'read',
                finance: 'manage',
                performance: 'read'
            }
        },
        technician: {
            displayName: 'التقني',
            description: 'إدارة أجهزة POS ومتابعة المكاتب بدون الوصول المالي.',
            permissions: {
                dashboard: 'read',
                offices: 'read',
                devices: 'manage'
            }
        },
        viewer: {
            displayName: 'العرض فقط',
            description: 'استعراض جميع الأقسام بدون إضافة أو تعديل أو حذف.',
            permissions: {
                dashboard: 'read',
                search: 'read',
                offices: 'read',
                devices: 'read',
                finance: 'read',
                users: 'read',
                roles: 'read',
                reports: 'read',
                performance: 'read'
            }
        }
    },

    pageRules: {
        'dashboard.html': { area: 'dashboard', access: 'read' },
        'search.html': { area: 'search', access: 'read' },
        'offices.html': { area: 'offices', access: 'read' },
        'office-details.html': { area: 'offices', access: 'read' },
        'create-library.html': { area: 'offices', access: 'manage' },
        'office-edit.html': { area: 'offices', access: 'manage' },
        'office-account-add.html': { area: 'offices', access: 'manage' },
        'office-account-edit.html': { area: 'offices', access: 'manage' },
        'office-pos-add.html': { area: 'devices', access: 'manage' },
        'office-pos-edit.html': { area: 'devices', access: 'manage' },
        'users.html': { area: 'users', access: 'read' },
        'user-details.html': { area: 'users', access: 'read' },
        'admin-user-add.html': { area: 'users', access: 'manage' },
        'admin-roles.html': { area: 'roles', access: 'manage' },
        'reports.html': { area: 'reports', access: 'read' },
        'performance.html': { area: 'performance', access: 'read' },
        'transaction-details.html': { area: 'finance', access: 'read' }
    },

    getStorage() {
        return window.sessionStorage;
    },

    getLegacyStorage() {
        return window.localStorage;
    },

    getLegacySessionObject() {
        try {
            return JSON.parse(this.getLegacyStorage().getItem('adminSession') || 'null');
        } catch {
            return null;
        }
    },

    writeLegacySessionValue(key, value) {
        const session = this.getLegacySessionObject() || {};
        session[key] = value;
        this.getLegacyStorage().setItem('adminSession', JSON.stringify(session));
    },

    removeLegacySessionValue(key) {
        const session = this.getLegacySessionObject();
        if (!session || typeof session !== 'object') {
            return;
        }

        delete session[key];
        const remainingKeys = Object.keys(session);
        if (!remainingKeys.length) {
            this.getLegacyStorage().removeItem('adminSession');
            return;
        }

        this.getLegacyStorage().setItem('adminSession', JSON.stringify(session));
    },

    setStoredValue(key, value) {
        if (value === undefined || value === null) {
            return;
        }

        const normalizedValue = String(value);
        this.getStorage().setItem(key, normalizedValue);
        this.getLegacyStorage().removeItem(key);
        this.writeLegacySessionValue(key, normalizedValue);
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
            this.writeLegacySessionValue(key, legacyValue);
            return legacyValue;
        }

        const legacySession = this.getLegacySessionObject();
        if (legacySession && legacySession[key] !== undefined && legacySession[key] !== null) {
            const value = String(legacySession[key]);
            this.getStorage().setItem(key, value);
            return value;
        }

        return null;
    },

    removeStoredValue(key) {
        this.getStorage().removeItem(key);
        this.getLegacyStorage().removeItem(key);
        this.removeLegacySessionValue(key);
    },

    clearStoredAuth() {
        this.storageKeys.forEach((key) => this.removeStoredValue(key));
        this.getStorage().removeItem('uiNotice');
        this.getLegacyStorage().removeItem('adminSession');
    },

    normalizeRoleCode(roleCode) {
        switch (String(roleCode || '').trim().toLowerCase()) {
            case 'super_admin':
            case 'admin':
            case 'general_manager':
                return 'general_manager';
            case 'accountant':
                return 'accountant';
            case 'technician':
            case 'technical':
            case 'tech':
                return 'technician';
            case 'viewer':
            case 'display':
            case 'read_only':
            case 'readonly':
                return 'viewer';
            default:
                return 'viewer';
        }
    },

    getRoleCode() {
        return this.getStoredValue('roleCode') || '';
    },

    getNormalizedRoleCode() {
        return this.normalizeRoleCode(this.getRoleCode());
    },

    getRoleProfile() {
        const roleKey = this.getNormalizedRoleCode();
        return this.roleProfiles[roleKey] || this.roleProfiles.viewer;
    },

    getUsername() {
        return this.getStoredValue('username') || 'admin';
    },

    getDisplayName() {
        return this.getRoleProfile().displayName;
    },

    getProfileDescription() {
        return this.getRoleProfile().description;
    },

    getPermissionLevel(area) {
        const profile = this.getRoleProfile();
        return profile.permissions[area] || 'none';
    },

    hasAccess(area, requiredAccess = 'read') {
        const granted = this.getPermissionLevel(area);
        const order = { none: 0, read: 1, manage: 2 };
        return (order[granted] || 0) >= (order[requiredAccess] || 0);
    },

    canAccessArea(area) {
        return this.hasAccess(area, 'read');
    },

    canManageArea(area) {
        return this.hasAccess(area, 'manage');
    },

    getCurrentPageName() {
        const path = String(window.location.pathname || '').replace(/\\/g, '/');
        const pageName = path.split('/').pop();
        return pageName ? pageName.toLowerCase() : 'index.html';
    },

    getCurrentPageRule() {
        return this.pageRules[this.getCurrentPageName()] || null;
    },

    setUiNotice(message) {
        if (!message) {
            this.getStorage().removeItem('uiNotice');
            return;
        }

        this.getStorage().setItem('uiNotice', String(message));
    },

    consumeUiNotice() {
        const value = this.getStorage().getItem('uiNotice');
        if (value !== null) {
            this.getStorage().removeItem('uiNotice');
        }

        return value;
    },

    saveTokens(data) {
        this.setStoredValue('accessToken', data.accessToken);
        this.setStoredValue('refreshToken', data.refreshToken);
        this.setStoredValue('expiresAt', data.expiresAt);
        this.setStoredValue('userId', data.userId);
        this.setStoredValue('username', data.username);
        this.setStoredValue('roleCode', data.roleCode || '');
    },

    getAccessToken() {
        return this.getStoredValue('accessToken');
    },

    getRefreshToken() {
        return this.getStoredValue('refreshToken');
    },

    isLoggedIn() {
        return !!this.getAccessToken();
    },

    logout() {
        this.clearStoredAuth();
        window.location.href = 'index.html';
    },

    redirectToDefaultPage(message) {
        if (message) {
            this.setUiNotice(message);
        }

        window.location.href = 'dashboard.html';
    },

    requireAuth() {
        if (!this.isLoggedIn()) {
            window.location.href = 'index.html';
            return false;
        }

        const currentPageRule = this.getCurrentPageRule();
        if (!currentPageRule) {
            return true;
        }

        if (this.hasAccess(currentPageRule.area, currentPageRule.access)) {
            return true;
        }

        const areaLabel = this.areaLabels[currentPageRule.area] || currentPageRule.area;
        const accessLabel = currentPageRule.access === 'manage' ? 'إدارة' : 'عرض';
        this.redirectToDefaultPage(`لا تملك صلاحية ${accessLabel} قسم ${areaLabel}.`);
        return false;
    },

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

    async apiFetch(url, options = {}) {
        let token = this.getAccessToken();
        if (!token) {
            this.logout();
            return null;
        }

        options.headers = {
            ...options.headers,
            Authorization: `Bearer ${token}`,
            'Content-Type': 'application/json'
        };

        let response = await fetch(url, options);

        if (response.status === 401) {
            token = await this.refreshAccessToken();
            if (!token) {
                return null;
            }

            options.headers.Authorization = `Bearer ${token}`;
            response = await fetch(url, options);
        }

        return response;
    }
};

Auth.areaLabels.roles = 'إعدادات الوصول';
if (Auth.roleProfiles.viewer) {
    Auth.roleProfiles.viewer.displayName = 'واجهة العرض';
}

window.Auth = Auth;
