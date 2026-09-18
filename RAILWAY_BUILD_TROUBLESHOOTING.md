# Railway Build Failure Troubleshooting

## 🔧 What I Fixed

1. **Dockerfile Location** - Moved to project root (Railway looks for it here)
2. **.dockerignore** - Created to exclude unnecessary files and speed up builds
3. **railway.toml** - Updated to explicitly point to root Dockerfile
4. **Start Command** - Simplified to run from app directory directly

---

## ✅ How to Redeploy

### **Option 1: Push Changes to GitHub (Recommended)**

```powershell
cd "C:\Users\ASPIRE\Downloads\VehicleProfitTracker_LayeredAPI\VPT\"

# Stage the new files
git add Dockerfile .dockerignore railway.toml

# Commit
git commit -m "Fix Railway Docker build configuration"

# Push to trigger automatic redeploy
git push origin main
```

Railway will automatically detect the new files and redeploy within 1-2 minutes.

### **Option 2: Manual Redeploy in Railway Dashboard**

1. Go to https://railway.app (login to your account)
2. Select your project
3. Go to **Deployments** tab
4. Click the **"Redeploy"** button on the latest failed deployment
5. Watch build progress in **Logs** tab

---

## 📋 Build Process Overview

Your new build process:

```
1. Railway pulls from GitHub ✅
2. Finds Dockerfile in root ✅
3. Uses .dockerignore to exclude unnecessary files ✅
4. Builds Docker image with SDK:8.0 ✅
5. Restores NuGet packages ✅
6. Compiles and publishes in Release mode ✅
7. Creates runtime image with aspnet:8.0 ✅
8. Copies published app ✅
9. Sets environment variables ✅
10. Starts on port 8080 ✅
```

---

## 🔍 Monitor the Build

Once you redeploy:

1. **Logs Tab** - Watch build output in real-time
2. **Expected Output** Should show:
   ```
   Building Docker image...
   dotnet restore output...
   dotnet publish output...
   Docker image built successfully
   Container started on port 8080
   ```

3. **Deployments Tab** - Shows build progress bar

---

## 🧪 Test After Deployment

Once Railway shows "✅ Success":

```powershell
# Replace <your-railway-url> with actual URL from Railway dashboard
# Example: https://vehicleprofittracker-production.railway.app

# Health check
curl https://<your-railway-url>/health

# Swagger API docs
curl https://<your-railway-url>/swagger

# Test login endpoint
curl -X POST https://<your-railway-url>/api/v1/auth/register `
  -H "Content-Type: application/json" `
  -d '{
	"name": "Test User",
	"phone": "+1234567890",
	"password": "SecurePass123!"
  }'
```

---

## ❌ If Build Still Fails

### **Check These Things:**

1. **Verify Dockerfile exists in root:**
   ```powershell
   Test-Path "C:\Users\ASPIRE\Downloads\VehicleProfitTracker_LayeredAPI\VPT\Dockerfile"
   ```

2. **Verify .dockerignore exists:**
   ```powershell
   Test-Path "C:\Users\ASPIRE\Downloads\VehicleProfitTracker_LayeredAPI\VPT\.dockerignore"
   ```

3. **Check railway.toml is correct:**
   ```powershell
   Get-Content "C:\Users\ASPIRE\Downloads\VehicleProfitTracker_LayeredAPI\VPT\railway.toml"
   ```
   Should show:
   ```toml
   [build]
   builder = "dockerfile"
   dockerfilePath = "Dockerfile"

   [deploy]
   startCommand = "dotnet VehicleProfitTracker.API.dll"
   ```

4. **Check .csproj file exists:**
   ```powershell
   Test-Path "API/VehicleProfitTracker.API.csproj"
   ```

---

## 🐛 Common Build Issues & Fixes

### **Issue: "Dockerfile not found"**
- **Fix:** Ensure Dockerfile is in project root, not in API/ folder
- **Verify:** `Dockerfile` and `Dockerfile.old` should both exist

### **Issue: "dotnet restore failed"**
- **Cause:** NuGet package source timeout or network issue
- **Fix:** Wait a few minutes and redeploy - Railway will retry

### **Issue: "dotnet publish failed" / "MSB3202 error"**
- **Cause:** Missing dependencies or build error in code
- **Fix:** 
  - Build locally first to verify: `dotnet publish API -c Release`
  - Check for compilation errors
  - Push fixes to GitHub and redeploy

### **Issue: Build starts but container won't start**
- **Cause:** Port already in use or missing environment variables
- **Fix:** Check Railway Variables tab - ensure all required vars are set:
  - `Jwt__Secret` (required!)
  - `ASPNETCORE_ENVIRONMENT=Production`
  - Database connection (Railway PostgreSQL auto-set this)

### **Issue: Container starts but /health returns 500**
- **Cause:** Database connection failed
- **Fix:**
  - Verify PostgreSQL is running in Railway
  - Check connection string in Variables
  - Check database migrations applied

---

## 📊 View Build Logs

### **In Railway Dashboard:**
1. Go to Deployments tab
2. Click failed deployment
3. Scroll through logs for errors
4. Common log sections:
   - Build phase (Docker image creation)
   - Runtime phase (app startup)

### **Using Railway CLI:**
```powershell
# Install Railway CLI
npm install -g @railway/cli

# Login
railway login

# Select project
railway init

# View logs
railway logs

# Follow logs (like tail -f)
railway logs --follow
```

---

## ✅ Expected Successful Deployment Output

```
[build] Building Docker image from Dockerfile
[build] Step 1/14 : FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
[build] Pulling from library/dotnet
[build] Digest: sha256:...
[build] Step 2/14 : WORKDIR /src
[build] Running in container...
[build] Restoring packages...
[build] Restore completed in XXms
[build] Building...
[build] Publishing...
[build] Successfully published to /app/publish
[build] Docker image built: sha256:xxxxx
[runtime] Starting container...
[runtime] Container listening on http://+:8080
[runtime] Application started. Press Ctrl+C to shut down.
[success] Deployment completed ✅
```

---

## 🚀 Next Steps

1. **Commit and push the fixed files:**
   ```powershell
   git add Dockerfile .dockerignore railway.toml
   git commit -m "Fix Railway Docker build"
   git push origin main
   ```

2. **Wait 1-2 minutes for automatic redeploy**

3. **Monitor Logs tab in Railway**

4. **Once green, test your API endpoints**

5. **Share the Railway URL with your team**

---

## 📞 Still Having Issues?

If the build still fails after these fixes:

1. **Check Railway Logs** for exact error message
2. **Build locally to verify:**
   ```powershell
   cd API
   dotnet restore
   dotnet build -c Release
   dotnet publish -c Release -o ./bin/Release/publish
   ```
3. **Share the exact error** from Railway Logs tab
4. **Verify GitHub connection** in Railway Settings

---

## 📁 File Structure Check

Your project should now have this structure:

```
VPT/
├── Dockerfile                    ✅ NEW (in root)
├── .dockerignore                 ✅ NEW (in root)
├── railway.toml                  ✅ UPDATED
├── RAILWAY_DEPLOYMENT.md
├── API/
│   ├── VehicleProfitTracker.API.csproj
│   ├── Program.cs
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   ├── Dockerfile               ⚠️ OLD (keep for reference)
│   └── ... (other files)
├── VehicleProfitTracker.sln
└── ... (other files)
```

The new Dockerfile in root will be used by Railway. The old one in API/ is kept for reference.

---

**Rails should now build successfully! Push your changes and watch the Deployments tab.** 🚀
