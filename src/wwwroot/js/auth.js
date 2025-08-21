/**
 * Authentication and Authorization utilities
 * Updated to work with HttpOnly cookies instead of localStorage
 */

class AuthManager {
    constructor() {
        this._currentUser = null;
        this._isAuthenticated = false;
        this._isLoading = false;
    }

    /**
     * Check authentication status by making a request to me endpoint
     */
    async checkAuthStatus() {

        try {
            this._isLoading = true;
            const response = await fetch('/api/auth/me', {
                method: 'GET',
                credentials: 'include', // Include cookies
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this._isAuthenticated = true;
                this._currentUser = {
                    id: data.user.id,
                    username: data.user.username,
                    email: data.user.email,
                    roles: data.user.roles || []
                };
                return true;
            } else {
                this._isAuthenticated = false;
                this._currentUser = null;
                return false;
            }
        } catch (error) {
            console.warn('Auth check failed:', error);
            this._isAuthenticated = false;
            this._currentUser = null;
            return false;
        } finally {
            this._isLoading = false;
        }
    }

    /**
     * Get the stored JWT token (deprecated - use cookies instead)
     */
    getToken() {
        console.warn('getToken() is deprecated. Token is now stored in HttpOnly cookies.');
        return null;
    }

    /**
     * Store the JWT token (deprecated - use cookies instead)
     */
    setToken(token) {
        console.warn('setToken() is deprecated. Token is now stored in HttpOnly cookies.');
    }

    /**
     * Remove the JWT token (deprecated - use logout endpoint instead)
     */
    removeToken() {
        console.warn('removeToken() is deprecated. Use logout endpoint instead.');
    }

    /**
     * Check if user is authenticated
     */
    async isAuthenticated() {
        if (!this._isAuthenticated) {
            await this.checkAuthStatus();
        }
        return this._isAuthenticated;
    }

    /**
     * Parse JWT token payload (deprecated)
     */
    parseToken(token) {
        console.warn('parseToken() is deprecated. User info is fetched from API.');
        return null;
    }

    /**
     * Get current user info
     */
    async getCurrentUser() {
        if (!await this.isAuthenticated()) {
            return null;
        }

        // If we don't have user info cached, fetch it
        if (!this._currentUser) {
            await this.checkAuthStatus();
        }

        return this._currentUser;
    }

    /**
     * Check if user has a specific role
     */
    async hasRole(roleName) {
        const user = await this.getCurrentUser();
        console.log('User:', user);
        if (!user || !user.roles) return false;

        // Handle both single role and array of roles
        const roles = Array.isArray(user.roles) ? user.roles : [user.roles];
        return roles.includes(roleName);
    }

    /**
     * Check if user is admin
     */
    async isAdmin() {
        return await this.hasRole('Admin');
    }

    /**
     * Check if user can access a specific user's data
     */
    async canAccessUserData(targetUserId) {
        const currentUser = await this.getCurrentUser();
        console.log('Current user:', currentUser);

        if (!currentUser) return false;

        // User can access their own data
        if (currentUser.id === parseInt(targetUserId)) return true;

        // Admin can access any user's data
        return await this.isAdmin();
    }

    /**
     * Redirect to login if not authenticated
     */
    async requireAuth(redirectUrl = null) {
        const isAuth = await this.isAuthenticated();
        if (!isAuth) {
            const currentUrl = redirectUrl || window.location.pathname;
            window.location.href = `/login?returnUrl=${encodeURIComponent(currentUrl)}`;
            return false;
        }
        return true;
    }

    /**
     * Require specific role or redirect
     */
    async requireRole(roleName, redirectUrl = '/') {
        if (!await this.requireAuth()) return false;

        if (!await this.hasRole(roleName)) {
            alert('Access denied. You do not have permission to access this page.');
            window.location.href = redirectUrl;
            return false;
        }
        return true;
    }

    /**
     * Require admin role or redirect
     */
    async requireAdmin(redirectUrl = '/users') {
        return await this.requireRole('Admin', redirectUrl);
    }

    /**
     * Logout user
     */
    async logout() {
        try {
            // Call logout endpoint to clear cookies
            await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'include'
            });
        } catch (error) {
            console.warn('Logout API call failed:', error);
        }

        // Clear local state
        this._isAuthenticated = false;
        this._currentUser = null;

        // Redirect to login
        window.location.href = '/login';
    }

    /**
     * Show/hide elements based on authentication
     */
    async updateUI() {
        const isAuth = await this.isAuthenticated();
        const isAdmin = await this.isAdmin();
        const user = await this.getCurrentUser();

        // Update authentication-based elements
        document.querySelectorAll('[data-auth-required]').forEach(el => {
            el.style.display = isAuth ? '' : 'none';
        });

        document.querySelectorAll('[data-auth-hidden]').forEach(el => {
            el.style.display = isAuth ? 'none' : '';
        });

        // Update admin-only elements
        document.querySelectorAll('[data-admin-only]').forEach(el => {
            el.style.display = isAdmin ? '' : 'none';
        });

        // Update user info
        if (user) {
            document.querySelectorAll('[data-user-name]').forEach(el => {
                el.textContent = user.username;
            });

            document.querySelectorAll('[data-user-email]').forEach(el => {
                el.textContent = user.email;
            });

            document.querySelectorAll('[data-user-roles]').forEach(el => {
                const roles = Array.isArray(user.roles) ? user.roles : [user.roles];
                el.textContent = roles.join(', ');
            });
        }
    }
}

// Create global instance
window.authManager = new AuthManager();

// Auto-update UI on page load
document.addEventListener('DOMContentLoaded', async function() {
    await window.authManager.updateUI();
});

// Listen for storage changes (login/logout in other tabs)
// Note: With HttpOnly cookies, we can't listen to storage changes directly
// The UI will be updated when the page is refreshed or when auth status changes

