# 🔧 FIX DATABASE CONNECTION ERROR

## ❌ The Problem

Your API deployed successfully, but it can't connect to Supabase PostgreSQL:

```
Failed to connect to ...:5432
Network is unreachable
```

**Why:** The pooler endpoint (`aws-0-ap-south-1.pooler.supabase.com`) requires SNI hostname, which isn't working from Railway's Docker container.

---

## ✅ The Fix

We need to use Supabase's **direct database host** instead of the pooler.

### **Step 1: Get Your Supabase Direct Connection String**

1. Go to: https://supabase.com/dashboard
2. Select your project
3. Go to **Settings** → **Database** → **Connection string**
4. Look for the **"Connection string"** section
5. In the dropdown, select **"Nodejs"** or **"Postgres"**
6. You'll see something like:
   ```
   postgresql://postgres:[YOUR_PASSWORD]@db.wjrawwjxzitltolrcums.supabase.co:5432/postgres
   ```

**IMPORTANT:** Copy the host: `db.wjrawwjxzitltolrcums.supabase.co` ✅

This is the **direct database host** (not the pooler)!

### **Step 2: Update Railway Environment Variables**

1. Go to: https://railway.app/dashboard
2. Select **ProfitTrackerB** project
3. Go to **Variables** tab
4. **Delete or clear** any old connection strings
5. Add NEW environment variable:

   **Name:** `DefaultConnection`

   **Value:** 
   ```
   Host=db.wjrawwjxzitltolrcums.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Amazings#Facts4321;SSL Mode=Require;Trust Server Certificate=true
   ```

   ⚠️ **Replace with YOUR actual values:**
   - `db.wjrawwjxzitltolrcums.supabase.co` ← Your Supabase host
   - `Amazings#Facts4321` ← Your Supabase password

6. Click **Save** or **Update**

### **Step 3: Redeploy**

1. Go to **Deployments** tab
2. Click **"Redeploy"** on the latest deployment
3. Wait ~5 minutes for the build to complete

---

## 📋 Connection String Parts

Break down of what you're setting:

```
Host=db.wjrawwjxzitltolrcums.supabase.co  ← Your Supabase host
Port=5432                                  ← Standard PostgreSQL port
Database=postgres                          ← Default Supabase database
Username=postgres                          ← Default Supabase user
Password=Amazings#Facts4321                ← Your database password
SSL Mode=Require                           ← Required for Supabase
Trust Server Certificate=true              ← Allow self-signed certs
```

---

## 🔍 How to Find Your Exact Supabase Credentials

### **In Supabase Dashboard:**

1. Go to: https://supabase.com/dashboard
2. Select your project
3. Click **Settings** icon (bottom left)
4. Go to **Database**
5. Look for:
   - **Host** - Your database host
   - **Port** - Usually 5432
   - **Database Name** - Usually "postgres"
   - **Username** - Usually "postgres"
   - **Password** - Your PostgreSQL password

### **Connection String Tab:**

You'll see pre-built connection strings for different languages:
- Nodejs
- Python
- Java
- etc.

Copy from URI format to get the exact values.

---

## ✅ After Redeploy - Test Connection

Once redeployed, test your API:

### **Test 1: Health Check**
```bash
curl https://your-railway-url.railway.app/health
```

Expected response:
```json
{
  "status": "ok",
  "db": "connected",
  "timestampUtc": "2026-09-18T..."
}
```

### **Test 2: Login (via Swagger)**
```
https://your-railway-url.railway.app/swagger
```
Try POST `/api/v1/auth/login` with test credentials

### **Test 3: View Vehicles**
```bash
curl https://your-railway-url.railway.app/api/v1/vehicles \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🚨 If Still Not Working

### **Check these things:**

1. **Is the password correct?**
   - Supabase passwords are case-sensitive
   - Special characters need to be URL-encoded

2. **Is the host correct?**
   - Should be `db.xxxxx.supabase.co` (NOT the pooler)
   - Double-check in Supabase dashboard

3. **Are SSL settings correct?**
   - `SSL Mode=Require` is correct for Supabase
   - Keep `Trust Server Certificate=true`

4. **Check Railway Logs:**
   - Go to Railway dashboard
   - Click your deployment
   - Go to **Logs** tab
   - Look for exact error message

---

## 🎯 What We're Fixing

### **Before (Broken):**
```
Railway Container
	 ↓
Pooler (AWS endpoint - network blocked)
	 ↓
❌ Connection Failed
```

### **After (Fixed):**
```
Railway Container
	 ↓
Direct Supabase Host (db.xxxxx.supabase.co)
	 ↓
✅ Connected!
	 ↓
PostgreSQL
```

---

## 📝 Summary

| Step | Action |
|------|--------|
| 1 | Find your Supabase direct host |
| 2 | Add `DefaultConnection` to Railway Variables |
| 3 | Set correct connection string with your credentials |
| 4 | Redeploy Railway |
| 5 | Test health endpoint |
| 6 | Verify database queries work |

---

**Do this now and your API will be fully operational!** ✅

