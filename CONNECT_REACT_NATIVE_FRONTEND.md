# 📱 CONNECT REACT NATIVE FRONTEND TO DEPLOYED API

## 📋 Prerequisites

Before connecting your frontend, complete this first:
- ✅ API deployed on Railway
- ✅ Database connection fixed (see `FIX_DATABASE_CONNECTION.md`)
- ✅ Health check working: `https://your-api-url/health` returns 200 ✅

---

## 🎯 Architecture Overview

```
React Native / Expo App
		↓
   smartphone/emulator
		↓
	  WiFi/Mobile Data
		↓
   Your Deployed .NET API
https://your-railway-url.railway.app
		↓
   PostgreSQL (Supabase)
		↓
	  Your data
```

---

## 🔑 Step 1: Get Your Railway API URL

1. Go to: https://railway.app/dashboard
2. Click **ProfitTrackerB** project
3. Look for your **public URL** (usually shown at top)
4. It looks like:
   ```
   https://vehicleprofittracker-production.railway.app
   ```

**Copy this URL - you'll need it!**

---

## 🔧 Step 2: Find Your React Native API Configuration

In your React Native project, find where you defined the API base URL.

**Typical locations:**

### **Option A: In a config file**
```
src/config/api.js    or
src/services/api.js  or
src/constants/urls.js
```

### **Option B: In a service**
```
src/services/authService.js
src/services/apiClient.js
```

### **Option C: In App.js or main file**
Look for something like:
```javascript
const API_URL = "http://localhost:5000"
const API_BASE_URL = "http://10.0.0.100:5001"
```

---

## 📝 Step 3: Update Your API URL

### **What to Change**

**OLD (localhost development):**
```javascript
const API_URL = "http://localhost:5000";
// or
const API_URL = "http://10.0.2.2:5000";  // Android emulator
// or
const API_URL = "http://192.168.x.x:5000"; // Your laptop IP
```

**NEW (production deployed):**
```javascript
const API_URL = "https://vehicleprofittracker-production.railway.app";
```

### **Complete Example**

If your current file looks like:

```javascript
// src/services/apiClient.js
export const API_BASE_URL = "http://localhost:5000";

export async function apiCall(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const response = await fetch(url, {
	...options,
	headers: {
	  "Content-Type": "application/json",
	  ...options.headers,
	},
  });
  return response.json();
}
```

**Change to:**

```javascript
// src/services/apiClient.js
export const API_BASE_URL = "https://vehicleprofittracker-production.railway.app";

export async function apiCall(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  const response = await fetch(url, {
	...options,
	headers: {
	  "Content-Type": "application/json",
	  ...options.headers,
	},
  });
  return response.json();
}
```

---

## 🌍 Step 4: Environment Variables (Recommended)

Instead of hardcoding URLs, use environment variables:

### **For Expo projects:**

**Create `.env` file in root:**
```
EXPO_PUBLIC_API_URL=https://vehicleprofittracker-production.railway.app
```

**Then in your code:**
```javascript
const API_URL = process.env.EXPO_PUBLIC_API_URL;
```

### **Benefits:**
- ✅ Easy to switch between development and production
- ✅ No need to change code for different environments
- ✅ Better security (secrets not in code)

### **Different URLs for different environments:**

```
// .env (git-ignored)
EXPO_PUBLIC_API_URL=https://vehicleprofittracker-production.railway.app

// .env.development (optional)
EXPO_PUBLIC_API_URL=http://localhost:5000

// .env.local
EXPO_PUBLIC_API_URL=http://192.168.1.100:5000
```

---

## 🧪 Step 5: Test the Connection

### **Test 1: Simple fetch request**

Open Expo console and run:

```javascript
const response = await fetch("https://vehicleprofittracker-production.railway.app/health");
const data = await response.json();
console.log(data);
```

**Expected output:**
```json
{
  "status": "ok",
  "db": "connected",
  "timestampUtc": "2026-09-18T..."
}
```

### **Test 2: Login request**

```javascript
const response = await fetch(
  "https://vehicleprofittracker-production.railway.app/api/v1/auth/login",
  {
	method: "POST",
	headers: {
	  "Content-Type": "application/json",
	},
	body: JSON.stringify({
	  phone: "+1234567890",
	  password: "YourPassword123!",
	  rememberMe: false,
	}),
  }
);
const data = await response.json();
console.log(data);
```

**Expected response (on success):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "some-refresh-token...",
  "user": {
	"id": "uuid",
	"name": "User Name",
	"phone": "+1234567890"
  }
}
```

---

## 🔐 Step 6: Handle Authentication Tokens

### **Storing JWT Tokens:**

```javascript
import AsyncStorage from "@react-native-async-storage/async-storage";

// After login
const response = await fetch(`${API_URL}/api/v1/auth/login`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify({ phone, password, rememberMe }),
});

const { accessToken, refreshToken, user } = await response.json();

// Store tokens
await AsyncStorage.setItem("accessToken", accessToken);
await AsyncStorage.setItem("refreshToken", refreshToken);
await AsyncStorage.setItem("user", JSON.stringify(user));
```

### **Using JWT in Requests:**

```javascript
// When making authenticated requests
const accessToken = await AsyncStorage.getItem("accessToken");

const response = await fetch(`${API_URL}/api/v1/vehicles`, {
  method: "GET",
  headers: {
	"Content-Type": "application/json",
	Authorization: `Bearer ${accessToken}`,
  },
});
```

---

## 🚀 Step 7: Deploy Your Frontend

Once testing is complete locally:

### **Expo Go (Development):**
```bash
expo start
# Scan QR code with Expo Go app
```

### **Build APK (Android):**
```bash
eas build --platform android --local
```

### **Build IPA (iOS):**
```bash
eas build --platform ios --local
```

---

## 🐛 Troubleshooting

### **"Network request failed" or "Failed to fetch"**

**Causes:**
- API URL is incorrect
- API is offline/crashed
- Firewall blocking requests
- CORS misconfigured

**Fix:**
1. Verify API URL in Railway dashboard
2. Test in browser: `https://your-api-url/health`
3. Check Railway logs for errors
4. Verify CORS settings in Railway Variables

### **"401 Unauthorized"**

**Cause:** JWT token is invalid or expired

**Fix:**
1. Re-login to get new token
2. Clear stored tokens: `AsyncStorage.removeItem("accessToken")`
3. Implement token refresh logic

### **"Connection refused" from Android Emulator**

**Issue:** `localhost` doesn't work in Android emulator

**Solution:**
- Use `http://10.0.2.2:5000` for emulator
- Use actual URL for real device
- Better: Use your production URL (works everywhere)

### **Slow response times**

**Causes:**
- Cold start on Railway (first request takes longer)
- Database query is slow
- Network latency

**Fix:**
- Implement loading spinners in UI
- Add request timeouts
- Implement retry logic

---

## ✅ Complete Checklist

- [ ] Database connection fixed on Railway
- [ ] Health check endpoint working
- [ ] Got Railway API URL
- [ ] Found API URL configuration in React Native code
- [ ] Updated all API URLs to production URL
- [ ] Tested health endpoint from Expo
- [ ] Tested login endpoint
- [ ] Tested authenticated endpoints (vehicles, transactions, etc.)
- [ ] Implement JWT token storage (AsyncStorage)
- [ ] Implement JWT in request headers
- [ ] Test on physical device (if available)
- [ ] Deploy to production

---

## 📚 Example File Structure

Your project should look like:

```
MyApp/
├── src/
│   ├── services/
│   │   ├── apiClient.js      ← API base URL here
│   │   ├── authService.js    ← Login/register
│   │   └── vehicleService.js ← Vehicle endpoints
│   ├── screens/
│   ├── components/
│   └── App.js
├── .env                       ← Environment variables
├── .env.example              ← Example (commit this)
├── app.json
└── package.json
```

---

## 🎉 You're Ready!

Once you:
1. Fix the database connection ✅
2. Update your API URLs ✅
3. Test the connection ✅

Your React Native app will be talking to your deployed .NET API!

---

## 📞 Need Help?

If something doesn't work:

1. Check Railway logs for API errors
2. Test API endpoints in Swagger: `https://your-api-url/swagger`
3. Check device network: Is it on WiFi/mobile data?
4. Enable console logging in your React Native app
5. Check for CORS errors in browser DevTools

