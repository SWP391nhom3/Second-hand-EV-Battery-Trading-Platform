# 📘 Google OAuth + OTP Authentication - Tài Liệu Tổng Hợp

## 📋 Mục Lục
1. [Tổng Quan Luồng](#tổng-quan-luồng)
2. [Luồng Chi Tiết](#luồng-chi-tiết)
3. [API Documentation](#api-documentation)
4. [Backend Configuration](#backend-configuration)
5. [Frontend Integration](#frontend-integration)
6. [Testing Guide](#testing-guide)
7. [Troubleshooting](#troubleshooting)

---

## 🎯 Tổng Quan Luồng

### Luồng Đăng Ký Với Google OAuth + OTP
1. **User click "Đăng nhập bằng Google"** → FE gọi `GET /api/Auth/google/start`
2. **Backend trả về Google OAuth URL** → FE redirect user đến Google
3. **User chọn Google account** → Google redirect về `/google-callback` với `code`
4. **FE gọi `GET /api/Auth/google/callback`** → Backend đổi code lấy user info
5. **Backend kiểm tra account:**
   - **Chưa tồn tại** → Trả `pendingToken` → FE điền password → Backend tạo account + gửi OTP
   - **Tồn tại nhưng chưa verified** → Trả `requiresOtp=true` → FE vào thẳng bước OTP
   - **Tồn tại và đã verified** → Trả `token` → FE login thành công
6. **User nhập OTP** → FE gọi `POST /api/Auth/otp/verify` → Backend verify và trả JWT token
7. **FE lưu token và fetch profile** → Lưu vào localStorage → Navigate về homepage

### Luồng Đăng Nhập Với Google OAuth
1. **User đã có account verified** → Sau bước 4 → Backend trả `token` trực tiếp
2. **FE lưu token và profile** → Navigate về homepage

---

## 🔄 Luồng Chi Tiết

### Flow 1: Đăng Ký Mới (Chưa Có Account)

```
┌─────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│   FE    │────▶│ Backend  │────▶│  Google  │────▶│   FE     │────▶│ Backend  │
│         │     │ /start   │     │   OAuth  │     │ callback │     │/callback│
└─────────┘     └──────────┘     └──────────┘     └──────────┘     └──────────┘
    │                                                                    │
    │                                                                    │
    │                             ┌──────────┐                          │
    │                             │   FE     │                          │
    │                         ───▶│/register │                          │
    │                             │complete  │                          │
    │                             └──────────┘                          │
    │                                 │                                  │
    │                                 │                                  │
    │                         ┌───────▼────────┐                        │
    │                         │  Backend tạo    │                        │
    │                         │  account + gửi  │                        │
    │                         │  OTP qua email  │                        │
    │                         └───────┬────────┘                        │
    │                                 │                                  │
    │                                 │                                  │
    │                         ┌───────▼────────┐                        │
    │                         │   FE nhập OTP  │                        │
    │                         └───────┬────────┘                        │
    │                                 │                                  │
    └─────────────────────────────────┼──────────────────────────────────┘
                                      │
                            ┌─────────▼──────────┐
                            │ POST /otp/verify   │
                            │ Backend verify     │
                            │ Set EmailVerified  │
                            │ Trả JWT token      │
                            └────────────────────┘
```

**Chi Tiết:**
1. User click "Đăng nhập bằng Google"
2. FE: `GET /api/Auth/google/start?state={random}` → Backend trả Google OAuth URL
3. FE: Redirect user đến Google → User chọn account → Google redirect về `/google-callback?code=...&state=...`
4. FE: `GET /api/Auth/google/callback?code=...&state=...` → Backend đổi code lấy user info
5. Backend: Account chưa tồn tại → Trả `{ mode: "register", pendingToken: "...", profile: {...} }`
6. FE: Navigate đến `/google-register` với pendingToken trong sessionStorage
7. User: Nhập password → FE: `POST /api/Auth/google/register-complete` → Backend tạo account + gửi OTP
8. FE: Chuyển sang bước nhập OTP
9. User: Nhập OTP → FE: `POST /api/Auth/otp/verify` → Backend verify, set EmailVerified=true, trả JWT token
10. FE: Lưu token, fetch profile, navigate về homepage

---

### Flow 2: Đăng Ký Lại (Account Chưa Verified, Quá 1 Phút)

```
Backend phát hiện account chưa verified và CreatedAt > 1 phút
→ Xóa account cũ (bao gồm Member, ExternalLogins, OtpCodes)
→ Trả về như Flow 1 (pendingToken để đăng ký lại)
```

---

### Flow 3: Xác Minh OTP Cho Account Chưa Verified

```
Account tồn tại nhưng EmailVerified = false và CreatedAt < 1 phút
→ Backend gửi OTP với purpose="Register"
→ FE nhận requiresOtp=true → Navigate thẳng đến bước OTP
→ User nhập OTP → Verify → Set EmailVerified=true → Trả token
```

---

### Flow 4: Đăng Nhập (Account Đã Verified)

```
Account tồn tại và EmailVerified = true
→ Backend trả token trực tiếp (không cần OTP)
→ FE lưu token và profile → Navigate về homepage
```

---

## 🔌 API Documentation

### Base URL
**Backend:** `http://localhost:{PORT}/api/Auth`  
**Frontend Callback:** `http://localhost:5173/google-callback`

**Lưu ý:** Thay `{PORT}` bằng port thực tế từ `Properties/launchSettings.json`

---

### 1. GET `/api/Auth/google/start`

**Mục đích:** Lấy Google OAuth authorization URL

**Query Parameters:**
- `state` (optional, string): State token để chống CSRF (nếu không có, backend tự generate)

**Request:**
```http
GET http://localhost:5000/api/Auth/google/start?state=abc123xyz
```

**Response:**
```json
{
  "url": "https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id=...&redirect_uri=...&scope=openid%20email%20profile&state=abc123xyz&prompt=select_account",
  "state": "abc123xyz"
}
```

**Frontend Usage:**
```javascript
const response = await fetch(`http://localhost:5000/api/Auth/google/start?state=${state}`);
const data = await response.json();
window.location.href = data.url; // Redirect to Google
```

---

### 2. GET `/api/Auth/google/callback`

**Mục đích:** Xử lý callback từ Google, đổi authorization code lấy user info, quyết định flow

**Query Parameters:**
- `code` (required, string): Authorization code từ Google
- `state` (required, string): State token để verify (phải khớp với state đã gửi)
- `redirectUri` (optional, string): Redirect URI (nếu không có, dùng từ config)

**Request:**
```http
GET http://localhost:5000/api/Auth/google/callback?code=4/0Ab32j90...&state=abc123xyz&redirectUri=http://localhost:5173/google-callback
```

**Response Cases:**

#### Case 1: Đăng Ký Mới (Account Chưa Tồn Tại)
```json
{
  "mode": "register",
  "pendingToken": "fc8aea0d076c44599d6dda3402ac8e48",
  "profile": {
    "email": "user@gmail.com",
    "name": "User Name",
    "avatar": "https://lh3.googleusercontent.com/a/..."
  }
}
```

#### Case 2: Account Chưa Verified (Còn Hạn < 1 Phút)
```json
{
  "mode": "register",
  "email": "user@gmail.com",
  "requiresOtp": true
}
```

#### Case 3: Account Chưa Verified (Quá 1 Phút - Xóa và Đăng Ký Lại)
```json
{
  "mode": "register",
  "pendingToken": "new_pending_token_after_deletion",
  "profile": {
    "email": "user@gmail.com",
    "name": "User Name",
    "avatar": "https://..."
  }
}
```

#### Case 4: Đăng Nhập (Account Đã Verified)
```json
{
  "mode": "login",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Frontend Usage:**
```javascript
const urlParams = new URLSearchParams(window.location.search);
const code = urlParams.get('code');
const state = urlParams.get('state');
const redirectUri = `${window.location.origin}/google-callback`;

const response = await fetch(
  `http://localhost:5000/api/Auth/google/callback?code=${code}&state=${state}&redirectUri=${encodeURIComponent(redirectUri)}`
);
const data = await response.json();

if (data.mode === 'register' && data.pendingToken) {
  // Navigate to registration form
} else if (data.mode === 'login' && data.token) {
  // Save token and login
}
```

---

### 3. POST `/api/Auth/google/register-complete`

**Mục đích:** Hoàn tất đăng ký với Google (sau khi có pendingToken)

**Request Body:**
```json
{
  "pendingToken": "fc8aea0d076c44599d6dda3402ac8e48",
  "fullName": "User Name",
  "password": "user_password_123"
}
```

**Response:**
```json
{
  "message": "OTP sent to email for registration",
  "email": "user@gmail.com"
}
```

**Lưu ý:** Sau khi gọi API này, backend tự động:
1. Tạo Account với `EmailVerified = false`
2. Tạo Member profile
3. Gửi OTP qua email với purpose="Register"
4. Frontend cần chuyển sang bước nhập OTP

**Frontend Usage:**
```javascript
const response = await fetch('http://localhost:5000/api/Auth/google/register-complete', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    pendingToken: pendingToken,
    fullName: formData.fullName,
    password: formData.password
  })
});
// After success, show OTP input step
```

---

### 4. POST `/api/Auth/otp/verify`

**Mục đích:** Xác minh OTP code (cho Register hoặc Login)

**Request Body:**
```json
{
  "email": "user@gmail.com",
  "code": "123456",
  "purpose": "Register"  // hoặc "Login"
}
```

**Response (Success):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (Error):**
```json
{
  "message": "Invalid or expired OTP"
}
```

**Lưu ý:**
- Với `purpose="Register"`: Backend tự động set `EmailVerified = true`
- Với `purpose="Login"`: Chỉ verify OTP, không thay đổi EmailVerified
- Sau khi verify thành công, trả về JWT token

**Frontend Usage:**
```javascript
const response = await fetch('http://localhost:5000/api/Auth/otp/verify', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: email,
    code: otpCode,
    purpose: 'Register' // hoặc 'Login'
  })
});

const data = await response.json();
if (data.token) {
  localStorage.setItem('token', data.token);
  // Fetch and save user profile
}
```

---

### 5. POST `/api/Auth/google/login-otp`

**Mục đích:** Gửi OTP đăng nhập cho tài khoản Google đã tồn tại (2FA)

**Request Body:**
```json
{
  "email": "user@gmail.com"
}
```

**Response:**
```json
{
  "message": "OTP sent",
  "email": "user@gmail.com"
}
```

**Frontend Usage:**
```javascript
// Optional: For 2FA on existing Google accounts
const response = await fetch('http://localhost:5000/api/Auth/google/login-otp', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: email })
});
```

---

## ⚙️ Backend Configuration

### 1. Google Cloud Console Setup

1. **Tạo OAuth 2.0 Client ID:**
   - Vào https://console.cloud.google.com/
   - APIs & Services → Credentials
   - Create Credentials → OAuth client ID
   - Application type: **Web application**
   - Authorized redirect URIs:
     ```
     http://localhost:5173/google-callback
     http://localhost:5000/api/Auth/google/callback
     ```

2. **Lấy Client ID và Client Secret:**
   - Copy `Client ID` và `Client Secret`
   - Lưu vào `appsettings.Development.json`

---

### 2. Cấu Hình Backend (`appsettings.Development.json`)

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
      "RedirectUri": "http://localhost:5173/google-callback"
    }
  },
  "EmailSettings": {
    "Smtp": {
      "Server": "smtp.gmail.com",
      "Port": 587,
      "EnableSsl": true,
      "Username": "your-email@gmail.com",
      "Password": "your-app-password"
    }
  }
}
```

**Lưu ý:**
- **RedirectUri** phải khớp 100% với redirect URI trong Google Cloud Console
- **Gmail App Password:** Nếu dùng Gmail, cần tạo App Password:
  1. Vào Google Account → Security
  2. Enable 2-Step Verification
  3. Generate App Password
  4. Dùng App Password (không phải mật khẩu Gmail)

---

### 3. JWT Configuration

**Trong `Program.cs`:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        // Configure JWT validation
    });
```

**Token được tạo trong `ITokenService.CreateJwt()`** với claims:
- `AccountId`
- `Email`
- `Role`

---

## 💻 Frontend Integration

### 1. Setup authService

**File:** `src/services/authService.js`

```javascript
import api from '../configs/axios';

export const authService = {
  // Google OAuth: Lấy URL và redirect
  googleLogin: async () => {
    const array = new Uint8Array(16);
    window.crypto.getRandomValues(array);
    const state = Array.from(array).map(b => b.toString(16).padStart(2, '0')).join('');
    sessionStorage.setItem('oauth_state', state);
    
    const resp = await fetch(`http://localhost:5000/api/Auth/google/start?state=${encodeURIComponent(state)}`);
    const data = await resp.json();
    if (data?.url) {
      window.location.href = data.url; // Redirect to Google
    }
  },

  // Hoàn tất đăng ký với Google
  registerWithGoogle: async (payload) => {
    const response = await api.post('/Auth/google/register-complete', payload);
    return response.data;
  },

  // Xác thực OTP
  verifyOtp: async (email, code, purpose) => {
    const response = await api.post('/Auth/otp/verify', {
      email,
      code,
      purpose // "Register" hoặc "Login"
    });
    return response.data;
  },

  // Gửi OTP đăng nhập (2FA)
  googleLoginOtp: async (email) => {
    const response = await api.post('/Auth/google/login-otp', { email });
    return response.data;
  }
};

// Helpers
export const authHelpers = {
  decodeJwt: (token) => {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload || {};
    } catch {
      return {};
    }
  },
  
  fetchProfileByEmail: async (email) => {
    try {
      const res = await api.get(`/Account/by-email/${encodeURIComponent(email)}`);
      return res.data;
    } catch {
      return null;
    }
  }
};
```

---

### 2. Google Callback Handler

**File:** `src/pages/google-callback/GoogleCallbackPage.jsx`

```javascript
import React, { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService, authHelpers } from '../../services/authService';

const GoogleCallbackPage = () => {
  const navigate = useNavigate();
  const didRunRef = useRef(false);

  useEffect(() => {
    if (didRunRef.current) return;
    didRunRef.current = true;

    const handleCallback = async () => {
      const urlParams = new URLSearchParams(window.location.search);
      const code = urlParams.get('code');
      const state = urlParams.get('state');
      const expectedState = sessionStorage.getItem('oauth_state');

      // Verify state (CSRF protection)
      if (expectedState && state && expectedState !== state) {
        navigate('/login', { replace: true });
        return;
      }

      if (!code) {
        navigate('/login', { replace: true });
        return;
      }

      const redirectUri = `${window.location.origin}/google-callback`;
      const qs = new URLSearchParams({ code, state, redirectUri }).toString();
      
      const response = await fetch(`http://localhost:5000/api/Auth/google/callback?${qs}`);
      const data = await response.json();

      sessionStorage.removeItem('oauth_state');

      // Case 1: Đăng ký mới
      if (data.mode === 'register' && data.pendingToken) {
        sessionStorage.setItem('google_pending_token', data.pendingToken);
        sessionStorage.setItem('google_profile', JSON.stringify(data.profile));
        navigate('/google-register', { replace: true });
        return;
      }

      // Case 2: Account chưa verified (vào thẳng OTP)
      if (data.mode === 'register' && data.requiresOtp && data.email) {
        sessionStorage.setItem('google_profile', JSON.stringify({ email: data.email }));
        navigate('/google-register', { 
          state: { otpOnly: true, email: data.email }, 
          replace: true 
        });
        return;
      }

      // Case 3: Đăng nhập thành công
      if (data.mode === 'login' && data.token) {
        localStorage.setItem('token', data.token);
        const payload = authHelpers.decodeJwt(data.token);
        if (payload?.email) {
          const profile = await authHelpers.fetchProfileByEmail(payload.email);
          if (profile) localStorage.setItem('user', JSON.stringify(profile));
        }
        navigate('/', { replace: true });
        return;
      }

      navigate('/', { replace: true });
    };

    handleCallback();
  }, [navigate]);

  return <div>Đang xử lý...</div>;
};

export default GoogleCallbackPage;
```

---

### 3. Google Registration Form

**File:** `src/components/auth/GoogleRegisterForm/GoogleRegisterForm.jsx`

**Flow:**
1. **Step 0:** Nhập password (nếu có pendingToken)
2. **Step 1:** Nhập OTP

```javascript
const [currentStep, setCurrentStep] = useState(0); // 0: password, 1: OTP

// Handle submit password
const handleSubmit = async (values) => {
  const response = await authService.registerWithGoogle({
    pendingToken: googleInfo.pendingToken,
    fullName: values.fullName || googleInfo.name,
    password: values.password
  });
  // Chuyển sang bước OTP
  setCurrentStep(1);
};

// Handle verify OTP
const handleVerifyOtp = async (values) => {
  const response = await authService.verifyOtp(
    googleInfo.email,
    values.otpCode,
    'Register'
  );
  
  if (response.token) {
    localStorage.setItem('token', response.token);
    const decodedToken = authHelpers.decodeJwt(response.token);
    const profile = await authHelpers.fetchProfileByEmail(decodedToken.email);
    if (profile) localStorage.setItem('user', JSON.stringify(profile));
    
    // Clear sessionStorage
    sessionStorage.removeItem('google_pending_token');
    sessionStorage.removeItem('google_profile');
    sessionStorage.removeItem('google_otp_only');
    
    navigate('/', { replace: true });
  }
};
```

---

## 🧪 Testing Guide

### 1. Test Backend APIs

#### Test 1: Lấy Google OAuth URL
```bash
curl "http://localhost:5000/api/Auth/google/start?state=test123"
```

**Expected:** Response có `url` và `state`

---

#### Test 2: Test Callback (Cần code thật từ Google)
**Lưu ý:** Cần test bằng browser vì cần Google OAuth flow thật

**Steps:**
1. Mở browser → `http://localhost:5000/api/Auth/google/start?state=test`
2. Copy URL từ response
3. Mở URL trong browser → Chọn Google account
4. Copy `code` từ redirect URL
5. Test callback:
```bash
curl "http://localhost:5000/api/Auth/google/callback?code=4/0Ab32j90...&state=test&redirectUri=http://localhost:5173/google-callback"
```

---

#### Test 3: Register Complete
```bash
curl -X POST "http://localhost:5000/api/Auth/google/register-complete" \
  -H "Content-Type: application/json" \
  -d '{
    "pendingToken": "test_token_from_callback",
    "fullName": "Test User",
    "password": "TestPassword123!"
  }'
```

**Expected:** `{ "message": "OTP sent to email for registration", "email": "..." }`

---

#### Test 4: Verify OTP
```bash
# Lấy OTP từ database (hoặc email nếu SMTP đã config)
SELECT TOP 1 Code FROM OtpCodes WHERE Email = 'test@gmail.com' AND Purpose = 'Register' ORDER BY CreatedAt DESC;

# Verify OTP
curl -X POST "http://localhost:5000/api/Auth/otp/verify" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@gmail.com",
    "code": "123456",
    "purpose": "Register"
  }'
```

**Expected:** `{ "token": "eyJhbGci..." }`

---

### 2. Test Frontend Flow

#### Test Đăng Ký Mới:
1. Click "Đăng nhập bằng Google"
2. Chọn Google account
3. Redirect về `/google-callback`
4. Tự động navigate đến `/google-register`
5. Nhập password
6. Nhập OTP (check email hoặc database)
7. Verify → Lưu token → Navigate về homepage

#### Test Đăng Nhập (Đã Verified):
1. Click "Đăng nhập bằng Google"
2. Chọn Google account đã verified
3. Redirect về `/google-callback`
4. Nhận token trực tiếp → Lưu token → Navigate về homepage

---

### 3. Test Database

**Kiểm tra Account sau khi đăng ký:**
```sql
SELECT AccountId, Email, GoogleId, EmailVerified, CreatedAt, LastLoginAt
FROM Accounts
WHERE Email = 'test@gmail.com';
```

**Kiểm tra OTP:**
```sql
SELECT OtpId, Email, Code, Purpose, CreatedAt, ExpiresAt, ConsumedAt
FROM OtpCodes
WHERE Email = 'test@gmail.com'
ORDER BY CreatedAt DESC;
```

---

## 🐛 Troubleshooting

### Lỗi 1: `redirect_uri_mismatch`

**Nguyên nhân:** Redirect URI không khớp giữa Google Console và Backend config

**Giải pháp:**
1. Kiểm tra Google Cloud Console → Authorized redirect URIs:
   ```
   http://localhost:5173/google-callback
   ```
2. Kiểm tra `appsettings.Development.json`:
   ```json
   "Authentication": {
     "Google": {
       "RedirectUri": "http://localhost:5173/google-callback"
     }
   }
   ```
3. Đảm bảo 100% khớp (không có trailing slash, đúng port, đúng protocol)

---

### Lỗi 2: `invalid_client` hoặc `The OAuth client was not found`

**Nguyên nhân:** Client ID hoặc Client Secret sai

**Giải pháp:**
1. Kiểm tra Google Cloud Console → Lấy đúng Client ID và Client Secret
2. Update `appsettings.Development.json`
3. Restart backend

---

### Lỗi 3: `State mismatch`

**Nguyên nhân:** State token không khớp giữa request và callback

**Giải pháp:**
- Đảm bảo FE lưu `state` vào `sessionStorage` trước khi redirect
- Verify `state` trước khi gọi callback API

---

### Lỗi 4: `Invalid or expired OTP`

**Nguyên nhân:**
- OTP đã hết hạn (5 phút)
- OTP đã được sử dụng
- Code không đúng

**Giải pháp:**
1. Kiểm tra OTP trong database:
   ```sql
   SELECT * FROM OtpCodes WHERE Email = '...' AND Purpose = 'Register' ORDER BY CreatedAt DESC;
   ```
2. Đảm bảo code đúng (6 chữ số)
3. Check `ExpiresAt` và `ConsumedAt`

---

### Lỗi 5: Email không được gửi (SMTP)

**Nguyên nhân:** SMTP config sai hoặc Gmail chặn

**Giải pháp:**
1. **Development:** Code đã bỏ qua lỗi email (OTP vẫn lưu vào DB)
   - Check OTP trong database để test

2. **Production:** Cần config SMTP đúng:
   - Gmail App Password (không dùng mật khẩu Gmail)
   - Hoặc dùng SMTP service khác (SendGrid, AWS SES, etc.)

---

### Lỗi 6: Account không được verify sau khi nhập OTP

**Nguyên nhân:** `purpose` không phải "Register"

**Giải pháp:**
- Đảm bảo gọi `POST /api/Auth/otp/verify` với `purpose: "Register"` cho luồng đăng ký
- Backend tự động set `EmailVerified = true` khi `purpose = "Register"`

---

### Lỗi 7: Token không hợp lệ hoặc thiếu claims

**Nguyên nhân:** JWT config sai hoặc thiếu claims

**Giải pháp:**
- Kiểm tra `ITokenService.CreateJwt()` đảm bảo có đầy đủ claims:
  - `AccountId`
  - `Email`
  - `Role`

---

## 📝 Checklist Integration

### Backend:
- [ ] Google Cloud Console đã config Client ID/Secret
- [ ] `appsettings.Development.json` đã điền Client ID/Secret
- [ ] RedirectUri khớp 100% với Google Console
- [ ] SMTP config (hoặc bỏ qua cho dev)
- [ ] JWT configuration đã setup

### Frontend:
- [ ] Route `/google-callback` đã setup
- [ ] Route `/google-register` đã setup
- [ ] `authService.googleLogin()` đã implement
- [ ] `GoogleCallbackPage` đã handle tất cả cases
- [ ] `GoogleRegisterForm` đã implement password + OTP steps
- [ ] State verification (CSRF protection)
- [ ] Token lưu vào localStorage
- [ ] User profile fetch và lưu sau khi login

---

## 🎯 Best Practices

1. **State Verification:** Luôn verify `state` token để chống CSRF
2. **Error Handling:** Handle tất cả error cases (invalid code, expired OTP, etc.)
3. **Session Management:** Clear sessionStorage sau khi hoàn tất flow
4. **Token Storage:** Lưu token vào localStorage, không lưu vào sessionStorage
5. **Profile Hydration:** Sau khi login, fetch và lưu full user profile vào localStorage
6. **Redirect After Login:** Navigate về homepage hoặc intended route

---

## 📚 Files Liên Quan

### Backend:
- `Controller/AuthController.cs` - Xử lý OAuth và OTP
- `Services/GoogleOAuthService.cs` - Google OAuth logic
- `Services/OtpService.cs` - OTP generation và verification
- `Services/EmailService.cs` - SMTP email sending
- `Services/TokenService.cs` - JWT token generation

### Frontend:
- `src/services/authService.js` - API calls
- `src/pages/google-callback/GoogleCallbackPage.jsx` - Callback handler
- `src/pages/google-register/GoogleRegisterPage.jsx` - Registration page
- `src/components/auth/GoogleRegisterForm/GoogleRegisterForm.jsx` - Registration form

---

**Documentation Version:** 1.0  
**Last Updated:** 2025-11-01

