// Auth helper functions for EVehicle API
const API_BASE_URL = 'http://localhost:9190/api';

// Token management
function setTokens(accessToken, refreshToken) {
    if (accessToken) {
        localStorage.setItem('access_token', accessToken);
    }
    if (refreshToken) {
        localStorage.setItem('refresh_token', refreshToken);
    }
}

function getAccessToken() {
    return localStorage.getItem('access_token');
}

function getRefreshToken() {
    return localStorage.getItem('refresh_token');
}

function clearTokens() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('jwt_token'); // Remove old token if exists
}

// Refresh access token using refresh token
async function refreshAccessToken() {
    const refreshToken = getRefreshToken();
    if (!refreshToken) {
        console.warn('No refresh token found');
        return null;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/auth/refresh-token`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                refreshToken: refreshToken
            })
        });

        if (response.ok) {
            const data = await response.json();
            if (data.accessToken && data.refreshToken) {
                setTokens(data.accessToken, data.refreshToken);
                console.log('Access token refreshed successfully');
                return data.accessToken;
            }
        } else {
            const errorData = await response.json();
            console.error('Failed to refresh token:', errorData);
            // Refresh token invalid, clear tokens
            clearTokens();
        }
    } catch (error) {
        console.error('Error refreshing token:', error);
        clearTokens();
    }

    return null;
}

// Make authenticated API request with auto token refresh
async function authenticatedFetch(url, options = {}) {
    let accessToken = getAccessToken();
    
    // Set Authorization header
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };
    
    if (accessToken) {
        headers['Authorization'] = `Bearer ${accessToken}`;
    }

    // Make request
    let response = await fetch(url, {
        ...options,
        headers
    });

    // If 401, try to refresh token and retry
    if (response.status === 401 && accessToken) {
        console.log('Access token expired, refreshing...');
        const newAccessToken = await refreshAccessToken();
        
        if (newAccessToken) {
            // Retry with new token
            headers['Authorization'] = `Bearer ${newAccessToken}`;
            response = await fetch(url, {
                ...options,
                headers
            });
        }
    }

    return response;
}

// Check if user is logged in
function isLoggedIn() {
    return !!getAccessToken();
}

// Get current user info
async function getCurrentUser() {
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/auth/me`);
        if (response.ok) {
            return await response.json();
        }
    } catch (error) {
        console.error('Error getting current user:', error);
    }
    return null;
}


