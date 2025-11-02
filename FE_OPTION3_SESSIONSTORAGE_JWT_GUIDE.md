# 📘 HƯỚNG DẪN FRONTEND: Option 3 - SessionStorage + JWT Decode

## 🎯 Tổng quan

Option 3 kết hợp **sessionStorage** (session-based) và **JWT decode** (validate token) để:
- ✅ Session-based storage (mất khi đóng tab)
- ✅ Validate token expiration trên FE
- ✅ Decode JWT để lấy claims (accountId, email, role)
- ✅ Auto-logout khi token hết hạn

---

## 📋 BACKEND ĐÃ CÓ SẴN

### 1. **JWT Token Service** (`JwtTokenService.cs`)

Backend đã tạo JWT token với các claims sau:

```csharp
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),  // accountId
    new Claim(JwtRegisteredClaimNames.Email, email ?? string.Empty), // email
    new Claim(ClaimTypes.Role, role ?? string.Empty) // role
};
```

**Token structure:**
- **Sub (Subject)**: `accountId` (string)
- **Email**: `email` (string)
- **Role**: `role` (string) - "Admin", "Staff", "Member"
- **Expiration**: 12 giờ (default)

### 2. **Login Endpoint** (`POST /api/Auth/login`)

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "account": {
    "accountId": 1,
    "email": "user@example.com",
    "phone": "",
    "role": "Member",
    "member": {
      "memberId": 1,
      "fullName": "John Doe",
      "avatarUrl": "https://...",
      "rating": 4.5
    }
  }
}
```

### 3. **JWT Bearer Authentication**

Backend đã config JWT Bearer authentication với:
- `ValidateIssuerSigningKey = true`
- `ValidateLifetime = true` (kiểm tra expiration)
- `ClockSkew = 5 minutes` (tolerance cho clock difference)

---

## 📝 FRONTEND CẦN LÀM

### Bước 1: Install `jwt-decode` package

```bash
npm install jwt-decode
```

hoặc

```bash
yarn add jwt-decode
```

hoặc

```bash
pnpm add jwt-decode
```

### Bước 2: Tạo `utils/jwt.js` - JWT decode và validate utilities

Tạo file `src/utils/jwt.js`:

```javascript
import { jwtDecode } from "jwt-decode";

/**
 * ✅ Decode JWT token để lấy thông tin user
 * @param {string} token - JWT token
 * @returns {object|null} - Decoded token payload hoặc null nếu invalid
 */
export const decodeToken = (token) => {
  try {
    if (!token) return null;
    return jwtDecode(token);
  } catch (error) {
    console.error("❌ Error decoding token:", error);
    return null;
  }
};

/**
 * ✅ Kiểm tra token có hết hạn chưa
 * @param {string} token - JWT token
 * @returns {boolean} - true nếu token đã hết hạn, false nếu còn hợp lệ
 */
export const isTokenExpired = (token) => {
  try {
    const decoded = decodeToken(token);
    if (!decoded || !decoded.exp) return true;

    const currentTime = Date.now() / 1000; // Convert to seconds
    return decoded.exp < currentTime;
  } catch (error) {
    console.error("❌ Error checking token expiration:", error);
    return true;
  }
};

/**
 * ✅ Lấy thông tin user từ token (extract claims)
 * @param {string} token - JWT token
 * @returns {object|null} - User info { accountId, email, role } hoặc null
 */
export const getUserFromToken = (token) => {
  try {
    const decoded = decodeToken(token);
    if (!decoded) return null;

    return {
      accountId: decoded.sub || decoded.accountId || null, // Sub claim = accountId
      email: decoded.email || decoded.Email || null,
      role: decoded.role || decoded.Role || decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || null,
    };
  } catch (error) {
    console.error("❌ Error getting user from token:", error);
    return null;
  }
};

/**
 * ✅ Validate token (kiểm tra expiration và format)
 * @param {string} token - JWT token
 * @returns {boolean} - true nếu token hợp lệ, false nếu không
 */
export const validateToken = (token) => {
  if (!token || typeof token !== "string") return false;
  if (token.trim() === "") return false;
  if (isTokenExpired(token)) return false;
  return true;
};
```

### Bước 3: Tạo `utils/sessionStorage.js` - Session-based storage utilities

Tạo file `src/utils/sessionStorage.js`:

```javascript
/**
 * ✅ Session Storage utilities - Data sẽ mất khi đóng browser tab
 * Sử dụng cho session-based storage (thay vì localStorage persistent)
 */

/**
 * Lưu token vào sessionStorage
 */
export const saveToken = (token) => {
  try {
    sessionStorage.setItem("token", token);
    return true;
  } catch (error) {
    console.error("❌ Error saving token to sessionStorage:", error);
    return false;
  }
};

/**
 * Lấy token từ sessionStorage
 */
export const getToken = () => {
  try {
    return sessionStorage.getItem("token");
  } catch (error) {
    console.error("❌ Error getting token from sessionStorage:", error);
    return null;
  }
};

/**
 * Lưu user info vào sessionStorage
 */
export const saveUser = (user) => {
  try {
    sessionStorage.setItem("user", JSON.stringify(user));
    return true;
  } catch (error) {
    console.error("❌ Error saving user to sessionStorage:", error);
    return false;
  }
};

/**
 * Lấy user info từ sessionStorage
 */
export const getUser = () => {
  try {
    const userStr = sessionStorage.getItem("user");
    return userStr ? JSON.parse(userStr) : null;
  } catch (error) {
    console.error("❌ Error getting user from sessionStorage:", error);
    return null;
  }
};

/**
 * Lưu role vào sessionStorage
 */
export const saveRole = (role) => {
  try {
    sessionStorage.setItem("role", role);
    return true;
  } catch (error) {
    console.error("❌ Error saving role to sessionStorage:", error);
    return false;
  }
};

/**
 * Lấy role từ sessionStorage
 */
export const getRole = () => {
  try {
    return sessionStorage.getItem("role");
  } catch (error) {
    console.error("❌ Error getting role from sessionStorage:", error);
    return null;
  }
};

/**
 * Xóa tất cả auth data từ sessionStorage
 */
export const clearSession = () => {
  try {
    sessionStorage.removeItem("token");
    sessionStorage.removeItem("user");
    sessionStorage.removeItem("role");
    return true;
  } catch (error) {
    console.error("❌ Error clearing sessionStorage:", error);
    return false;
  }
};

/**
 * Kiểm tra có đang logged in không (dựa trên token)
 */
export const isLoggedIn = () => {
  const token = getToken();
  return token !== null && token.trim() !== "";
};
```

### Bước 4: Cập nhật `LoginForm.jsx` - Validate + decode JWT khi login

Cập nhật `src/components/auth/LoginForm/LoginForm.jsx`:

```javascript
import React, { useState } from "react";
import { Button, Checkbox, Form, Input, Typography, Alert } from "antd";
import { MailOutlined, LockOutlined } from "@ant-design/icons";
import { useNavigate, Link } from "react-router-dom";
import { toast } from "react-toastify";
import api from "../../../configs/axios";
import { validateToken, getUserFromToken } from "../../../utils/jwt";
import { saveToken, saveUser, saveRole, clearSession } from "../../../utils/sessionStorage";
import styles from "./LoginForm.module.css";

const { Title, Text } = Typography;

const LoginForm = () => {
  const navigate = useNavigate();
  const [form] = Form.useForm();
  const [errorMessage, setErrorMessage] = useState("");

  const handleSubmit = async (values) => {
    setErrorMessage("");
    try {
      // 🔹 Gọi API đăng nhập
      const response = await api.post("api/Auth/login", {
        email: values.email,
        password: values.password,
      });

      const data = response.data || {};

      // 🔹 Validate response
      if (!data.token || !data.account) {
        throw new Error("Dữ liệu trả về không hợp lệ");
      }

      const token = data.token;
      const account = data.account;

      // ✅ Option 3: Validate token trước khi lưu
      if (!validateToken(token)) {
        throw new Error("Token không hợp lệ hoặc đã hết hạn");
      }

      // ✅ Decode JWT để lấy thông tin (fallback nếu account không có đủ thông tin)
      const tokenUser = getUserFromToken(token);
      const role = (account.role || tokenUser?.role || "Member")?.toLowerCase();

      // ✅ Lưu vào sessionStorage (session-based - mất khi đóng tab)
      saveToken(token);
      saveUser(account);
      saveRole(role);

      // ✅ Log để debug
      console.log("✅ Login successful:", {
        token: token ? "✓ Saved to sessionStorage" : "✗ Missing",
        user: account,
        role: role,
        tokenClaims: tokenUser,
      });

      // ✅ Cập nhật Header và components khác
      try {
        window.dispatchEvent(new Event("authChanged"));
      } catch (e) {
        console.warn("Could not dispatch authChanged event:", e);
      }

      toast.success("Đăng nhập thành công! 🎉");

      // ✅ Điều hướng theo role
      switch (role) {
        case "admin":
          navigate("/admin");
          break;
        case "staff":
          navigate("/staff");
          break;
        default:
          navigate("/");
          break;
      }
    } catch (error) {
      console.error("❌ Login error:", error);
      const msg =
        error.response?.data?.message ||
        error.message ||
        "Đăng nhập thất bại. Vui lòng thử lại.";
      setErrorMessage(msg);
      toast.error(msg);
    }
  };

  return (
    <div className={styles.loginFormContainer}>
      {/* ... rest of your form JSX ... */}
    </div>
  );
};

export default LoginForm;
```

### Bước 5: Cập nhật `axios.js` - Request/Response interceptors

Cập nhật `src/configs/axios.js`:

```javascript
import axios from "axios";
import { getToken } from "../utils/sessionStorage";
import { validateToken } from "../utils/jwt";

// Use environment variable for API URL, fallback to default
const apiBaseURL =
  import.meta.env.VITE_API_BASE_URL || "https://localhost:5001";

const api = axios.create({
  baseURL: apiBaseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

// ✅ Request interceptor: Tự động thêm token vào mọi request
api.interceptors.request.use(
  (config) => {
    // Lấy token từ sessionStorage (ưu tiên), fallback về localStorage
    const token =
      sessionStorage.getItem("token") ||
      localStorage.getItem("token") ||
      localStorage.getItem("authToken");

    if (token) {
      // ✅ Validate token trước khi gửi
      if (!validateToken(token)) {
        console.warn("⚠️ Token không hợp lệ hoặc đã hết hạn, sẽ clear session");
        // Clear invalid token
        sessionStorage.removeItem("token");
        localStorage.removeItem("token");
        localStorage.removeItem("authToken");
        localStorage.removeItem("user");
        localStorage.removeItem("role");

        // Redirect về login nếu không phải trang login
        if (window.location.pathname !== "/login") {
          window.location.href = "/login";
        }
        return Promise.reject(new Error("Token không hợp lệ"));
      }

      // Thêm token vào Authorization header
      config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// ✅ Response interceptor: Xử lý lỗi 401 (Unauthorized)
api.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // Nếu token hết hạn hoặc không hợp lệ, xóa token và redirect về login
    if (error.response?.status === 401) {
      // Clear tất cả auth data
      sessionStorage.removeItem("token");
      sessionStorage.removeItem("user");
      sessionStorage.removeItem("role");
      localStorage.removeItem("token");
      localStorage.removeItem("authToken");
      localStorage.removeItem("user");
      localStorage.removeItem("role");

      // Thông báo các component khác
      try {
        window.dispatchEvent(new Event("authChanged"));
      } catch (e) {
        console.warn("Could not dispatch authChanged event:", e);
      }

      // Chỉ redirect nếu không phải trang login
      if (window.location.pathname !== "/login") {
        window.location.href = "/login";
      }
    }

    return Promise.reject(error);
  }
);

export default api;
```

### Bước 6: Cập nhật `Header.jsx` - Đọc từ sessionStorage

Cập nhật `src/components/layout/Header/Header.jsx`:

```javascript
import React, { useState, useEffect } from "react";
import { Button, Badge, Avatar, Dropdown, message } from "antd";
import { UserOutlined, LogoutOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";
import { getToken, getUser, getRole, clearSession, isLoggedIn } from "../../../utils/sessionStorage";
import { validateToken } from "../../../utils/jwt";

const Header = () => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const navigate = useNavigate();

  // ✅ Option 3: Kiểm tra trạng thái đăng nhập với sessionStorage + validate token
  useEffect(() => {
    const checkAuth = async () => {
      try {
        // ✅ Ưu tiên sessionStorage, fallback về localStorage
        const token = getToken() || localStorage.getItem("token") || localStorage.getItem("authToken");

        if (token) {
          // ✅ Validate token (kiểm tra expiration)
          if (validateToken(token)) {
            setIsLoggedIn(true);
          } else {
            // Token hết hạn hoặc invalid
            setIsLoggedIn(false);
            // Clear invalid token
            clearSession();
            localStorage.removeItem("token");
            localStorage.removeItem("authToken");
            localStorage.removeItem("user");
            localStorage.removeItem("role");
          }
        } else {
          setIsLoggedIn(false);
        }
      } catch (error) {
        console.error("❌ Error checking auth:", error);
        setIsLoggedIn(false);
      }
    };
    checkAuth();
    window.addEventListener("authChanged", checkAuth);
    return () => window.removeEventListener("authChanged", checkAuth);
  }, []);

  // ✅ Option 3: Lấy thông tin user từ sessionStorage (ưu tiên), fallback về localStorage
  const getUserInfo = () => {
    try {
      // ✅ Ưu tiên sessionStorage, fallback về localStorage
      const user = getUser() || JSON.parse(localStorage.getItem("user") || "{}");
      if (!user || Object.keys(user).length === 0) {
        return {
          name: "User",
          avatar: `https://ui-avatars.com/api/?name=U&background=1890ff&color=fff`,
        };
      }
      const name = user.member?.fullName || user.email?.split("@")[0] || "User";
      const avatar =
        user.member?.avatarUrl ||
        user.avatarUrl ||
        `https://ui-avatars.com/api/?name=${encodeURIComponent(
          name
        )}&background=1890ff&color=fff`;
      return { name, avatar };
    } catch {
      return {
        name: "User",
        avatar: `https://ui-avatars.com/api/?name=U&background=1890ff&color=fff`,
      };
    }
  };

  const { name: userName, avatar: userAvatar } = getUserInfo();
  // ✅ Ưu tiên sessionStorage, fallback về localStorage
  const role = (getRole() || localStorage.getItem("role"))?.toLowerCase();

  // Logout handler
  const handleLogout = async () => {
    try {
      // ✅ Option 3: Xóa tất cả auth data từ sessionStorage và localStorage
      clearSession();
      localStorage.removeItem("token");
      localStorage.removeItem("authToken");
      localStorage.removeItem("user");
      localStorage.removeItem("role");

      setIsLoggedIn(false);
      message.success("Đã đăng xuất!");
      window.dispatchEvent(new Event("authChanged"));
      navigate("/");
    } catch (error) {
      console.error("❌ Error during logout:", error);
      // Fallback: Xóa thủ công
      sessionStorage.clear();
      localStorage.clear();
      setIsLoggedIn(false);
      navigate("/");
    }
  };

  return (
    <header>
      {/* ... your header JSX ... */}
      {/* Avatar với user info */}
      {isLoggedIn && (
        <Dropdown
          menu={{
            items: [
              {
                key: "logout",
                icon: <LogoutOutlined />,
                label: "Đăng xuất",
                danger: true,
                onClick: handleLogout,
              },
            ],
          }}
        >
          <Avatar src={userAvatar} />
          <span>{userName}</span>
        </Dropdown>
      )}
      {/* ... rest of your header JSX ... */}
    </header>
  );
};

export default Header;
```

---

## ✅ Checklist Implementation

- [ ] Install `jwt-decode` package
- [ ] Tạo `utils/jwt.js` với các functions: `decodeToken`, `isTokenExpired`, `getUserFromToken`, `validateToken`
- [ ] Tạo `utils/sessionStorage.js` với các functions: `saveToken`, `getToken`, `saveUser`, `getUser`, `saveRole`, `getRole`, `clearSession`, `isLoggedIn`
- [ ] Cập nhật `LoginForm.jsx`: Validate token, decode JWT, lưu vào sessionStorage
- [ ] Cập nhật `axios.js`: Request interceptor validate token, Response interceptor handle 401
- [ ] Cập nhật `Header.jsx`: Đọc từ sessionStorage, validate token expiration
- [ ] Test login flow
- [ ] Test token expiration
- [ ] Test auto-logout khi token hết hạn

---

## 🧪 Testing

### 1. Test Login Flow

```javascript
// 1. Đăng nhập
// 2. Kiểm tra sessionStorage có token không
console.log(sessionStorage.getItem("token")); // Should have JWT token

// 3. Kiểm tra token có được decode đúng không
import { getUserFromToken } from "./utils/jwt";
const token = sessionStorage.getItem("token");
const user = getUserFromToken(token);
console.log(user); // { accountId: "1", email: "user@example.com", role: "Member" }
```

### 2. Test Token Expiration

```javascript
// 1. Kiểm tra token expiration
import { isTokenExpired, validateToken } from "./utils/jwt";
const token = sessionStorage.getItem("token");

console.log("Token expired:", isTokenExpired(token)); // false (nếu còn hợp lệ)
console.log("Token valid:", validateToken(token)); // true (nếu còn hợp lệ)

// 2. Giả lập token hết hạn (trong browser console)
// Set token expiration về quá khứ (không nên làm trong production!)
// Sau đó refresh trang, header sẽ auto-logout
```

### 3. Test Auto-Logout

```javascript
// 1. Đăng nhập
// 2. Mở browser console
// 3. Xóa token hoặc làm token hết hạn
sessionStorage.removeItem("token");

// 4. Thực hiện một API call
// → Axios interceptor sẽ phát hiện và redirect về /login
```

### 4. Test Session Storage

```javascript
// 1. Đăng nhập
// 2. Đóng tab
// 3. Mở lại tab mới
// 4. sessionStorage sẽ bị xóa (không còn token)
// → User phải đăng nhập lại
```

---

## 🎯 Lợi ích của Option 3

✅ **Session-based storage**: Data mất khi đóng tab (an toàn hơn)  
✅ **Validate token expiration**: Kiểm tra token hết hạn trên FE  
✅ **Decode JWT**: Lấy claims (accountId, email, role) từ token  
✅ **Auto-logout**: Tự động đăng xuất khi token hết hạn  
✅ **Fallback**: Vẫn hỗ trợ localStorage nếu cần  

---

## 📚 Tài liệu tham khảo

- [jwt-decode npm](https://www.npmjs.com/package/jwt-decode)
- [MDN: sessionStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/sessionStorage)
- [JWT.io - Decode JWT](https://jwt.io/)

---

## ❓ FAQ

**Q: Tại sao dùng sessionStorage thay vì localStorage?**  
A: sessionStorage mất khi đóng tab (an toàn hơn), localStorage persist đến khi user xóa.

**Q: Token hết hạn thì làm sao?**  
A: Axios interceptor sẽ phát hiện và tự động redirect về `/login`.

**Q: Có cần refresh token không?**  
A: Hiện tại backend chưa có refresh token. Token có thời hạn 12 giờ, user sẽ phải đăng nhập lại khi hết hạn.

**Q: Làm sao để test token expiration?**  
A: Có thể chỉnh sửa token expiration trong browser console (không nên trong production) hoặc đợi 12 giờ.

**Q: Nếu token không hợp lệ thì sao?**  
A: `validateToken()` sẽ return `false`, và axios interceptor sẽ clear session và redirect về login.

---

**Chúc bạn implement thành công! 🎉**

