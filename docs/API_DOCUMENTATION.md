# API Documentation

Welcome to the Recycling App API Documentation. This guide provides front-end developers with everything needed to integrate with our backend services.

## Base URLs & Environments

| Environment | URL | Description |
|---|---|---|
| **Local (IIS Express / Kestrel)** | `https://localhost:7278` | Used for standard local development |
| **Docker** | `http://localhost:8080` | Used when running the backend via Docker container |

---

## Authentication Flow

The API secures protected endpoints using **JSON Web Tokens (JWT)**. 

### How to Authenticate
1. Call `POST /api/auth/login` with your credentials.
2. The response will contain a `token` property.
3. For all protected endpoints, include this token in the HTTP `Authorization` header:
   ```http
   Authorization: Bearer <YOUR_JWT_TOKEN>
   ```

**Note:** Tokens expire **120 minutes** after issuance. If a 401 response is received, prompt the user to log in again.

---

## Error Handling Contract

The API follows a standardized error response format for predictable handling on the client side.

### Standard Error Response Format
```json
{
  "status": 400,
  "title": "Bad Request",
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Password": ["The Password must be at least 8 characters long."]
  }
}
```

### Common HTTP Status Codes

| Status Code | Meaning | Handling Strategy |
|-------------|---------|-------------------|
| **400 Bad Request** | Validation failed | Display `errors` dictionary to the user |
| **401 Unauthorized** | Missing or expired token | Redirect to Login screen |
| **403 Forbidden** | Lack of permissions | Show "Access Denied" screen |
| **404 Not Found** | Resource does not exist | Show standard 404 UI component |
| **500 Server Error** | Unexpected backend crash | Show a generic "Something went wrong" alert |

---

## Complete Endpoint Specifications

### 1. Register User
Registers a new user in the system.

- **URL:** `/api/auth/register`
- **Method:** `POST`
- **Auth Required:** No

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "password": "StrongPassword123!",
  "street": "123 Green St",
  "city": "Eco City",
  "buildingNumber": "4B"
}
```

**Validation Rules:**
| Field | Type | Required | Rules / Validation Messages |
|---|---|---|---|
| `fullName` | String | **Yes** | Maximum 100 characters. |
| `email` | String | **Yes** | Must be a valid email format. |
| `phoneNumber` | String | **Yes** | Must be valid international format (e.g., `+1234567890`). |
| `password` | String | **Yes** | Minimum 6 characters. |
| `street` | String | **Yes** | Cannot be empty. |
| `city` | String | **Yes** | Cannot be empty. |
| `buildingNumber`| String | **Yes** | Cannot be empty. |

**Success Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "User registered successfully",
  "userId": "guid-uuid-string"
}
```

**Error Response (400 Bad Request):** Returns standard validation errors contract.

---

### 2. Login
Authenticates a user and issues a JWT token.

- **URL:** `/api/auth/login`
- **Method:** `POST`
- **Auth Required:** No

**Request Body:**
```json
{
  "emailOrPhone": "john.doe@example.com",
  "password": "StrongPassword123!"
}
```

**Validation Rules:**
| Field | Type | Required | Rules / Validation Messages |
|---|---|---|---|
| `emailOrPhone` | String | **Yes** | Cannot be empty. Accepts either Email address or Phone number. |
| `password` | String | **Yes** | Cannot be empty. |

**Success Response (200 OK):**
```json
{
  "succeeded": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR..."
}
```

**Error Response (401 Unauthorized):**
```json
{
  "status": 401,
  "title": "Invalid credentials provided."
}
```

---

### 3. Forgot Password
Initiates the password reset process by generating a reset token.

- **URL:** `/api/auth/forgot-password`
- **Method:** `POST`
- **Auth Required:** No

**Request Body:**
```json
{
  "email": "john.doe@example.com"
}
```

**Validation Rules:**
| Field | Type | Required | Rules / Validation Messages |
|---|---|---|---|
| `email` | String | **Yes** | Must be a valid email format. |

**Success Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Password reset email sent."
}
```

---

### 4. Reset Password
Resets the user's password using the token provided via email.

- **URL:** `/api/auth/reset-password`
- **Method:** `POST`
- **Auth Required:** No

**Request Body:**
```json
{
  "email": "john.doe@example.com",
  "token": "reset-token-string-from-email",
  "newPassword": "NewStrongPassword123!"
}
```

**Validation Rules:**
| Field | Type | Required | Rules / Validation Messages |
|---|---|---|---|
| `email` | String | **Yes** | Must be a valid email format. |
| `token` | String | **Yes** | Cannot be empty. |
| `newPassword` | String | **Yes** | Minimum 6 characters. |

**Success Response (200 OK):**
```json
{
  "succeeded": true,
  "message": "Password reset successfully."
}
```

**Error Response (400 Bad Request):**
```json
{
  "status": 400,
  "title": "Invalid or expired token."
}
```

---

### 5. Get User Profile
Retrieves the profile information of the currently authenticated user.

- **URL:** `/api/profile`
- **Method:** `GET`
- **Auth Required:** Yes (`Authorization: Bearer <token>`)

**Success Response (200 OK):**
```json
{
  "id": "guid-uuid-string",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "street": "123 Green St",
  "city": "Eco City",
  "buildingNumber": "4B",
  "pointsBalance": 1500
}
```

---

## Postman Collection Hints

To streamline your API testing in Postman, we recommend the following setup:

### 1. Environment Setup
Create a new Postman Environment (e.g., "RecyclingApp - Local") and add these variables:
- `baseUrl`: `https://localhost:7278`
- `jwt_token`: leave the initial value blank

### 2. Automatic Token Chaining
In your **Login** request in Postman, go to the **Tests** tab and add this script:

```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    if (jsonData.token) {
        pm.environment.set("jwt_token", jsonData.token);
    }
}
```
This automatically captures the JWT token and saves it to your environment variables upon a successful login.

### 3. Using the Token
For all protected requests (like `GET /api/profile`), go to the **Authorization** tab, select **Bearer Token**, and set the token value to `{{jwt_token}}`. This will dynamically use the token you just received from the login endpoint.
