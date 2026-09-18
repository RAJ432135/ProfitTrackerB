# Railway Deployment Guide

## Prerequisites
- ✅ GitHub account with repo pushed to https://github.com/RAJ432135/ProfitTrackerB
- ✅ Railway account (free tier available at https://railway.app)
- Supabase PostgreSQL credentials (if using Supabase) or use Railway's PostgreSQL

---

## Step 1: Create Railway Account & Link GitHub

1. Go to https://railway.app
2. Sign up with GitHub (recommended for seamless integration)
3. Click **"Create New Project"**
4. Select **"Deploy from GitHub repo"**
5. Authorize Railway to access your GitHub
6. Select **RAJ432135/ProfitTrackerB** repository
7. Select **main** branch
8. Railway will automatically detect your `.dockerfile` and start building

---

## Step 2: Add PostgreSQL Database

### Option A: Use Railway's PostgreSQL (Recommended)

1. In your Railway project dashboard, click **"+ Add"**
2. Select **"Database"** → **"PostgreSQL"**
3. Railway creates a PostgreSQL instance automatically
4. Railway injects `DATABASE_URL` environment variable into your app
5. **This is automatically connected!** No manual configuration needed.

### Option B: Use Supabase PostgreSQL (If Preferred)

1. In Railway project, go to **Variables** tab
2. Click **"New Variable"**
3. Add:
   ```
   Name: DefaultConnection
   Value: Host=your-project-ref.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
   ```
4. Replace `your-project-ref.supabase.co` and `YOUR_PASSWORD` with actual Supabase credentials

---

## Step 3: Configure Environment Variables

Go to **Variables** tab in your Railway project and add these:

### Required Variables

```
JWT_SECRET: your-super-secret-key-min-32-chars-CHANGE_THIS_IN_PRODUCTION
JWT_ISSUER: VehicleProfitTracker
JWT_AUDIENCE: VehicleProfitTrackerClient
JWT_ACCESSTOKENEXPIRYMINUTES: 60
```

### Optional Variables

```
ASPNETCORE_ENVIRONMENT: Production
LOG_LEVEL: Information
```

### Example Configuration:

| Variable | Value |
|----------|-------|
| `DATABASE_URL` | `postgresql://user:pass@host:5432/dbname` (Auto-set by Railway if using Railway PostgreSQL) |
| `Jwt__Secret` | `your-32-character-minimum-secure-random-string` |
| `Jwt__Issuer` | `VehicleProfitTracker` |
| `Jwt__Audience` | `VehicleProfitTrackerClient` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

---

## Step 4: Configure CORS for Production

Update your allowed origins in Railway Variables:

1. Go to **Variables** tab
2. Add:
   ```
   Cors__AllowedOrigins__0: https://your-frontend-domain.com
   Cors__AllowedOrigins__1: https://www.your-frontend-domain.com
   ```

---

## Step 5: Deploy & Monitor

1. Push your changes to GitHub `main` branch
   ```powershell
   git add .
   git commit -m "Add Railway configuration"
   git push origin main
   ```

2. Railway automatically redeploys whenever you push to `main`

3. Check **Deployments** tab to see build progress

4. Once deployed, Railway provides a public URL like:
   ```
   https://vehicleprofittracker-production.railway.app
   ```

---

## Step 6: Verify Deployment

1. **Health Check:**
   ```
   https://your-railway-url/health
   ```
   Should return:
   ```json
   {
	 "status": "ok",
	 "db": "connected",
	 "timestampUtc": "2025-09-12T..."
   }
   ```

2. **Swagger API Docs:**
   ```
   https://your-railway-url/swagger
   ```

3. **Test Registration:**
   ```bash
   curl -X POST https://your-railway-url/api/v1/auth/register \
	 -H "Content-Type: application/json" \
	 -d '{
	   "name": "Test User",
	   "phone": "+1234567890",
	   "password": "SecurePass123"
	 }'
   ```

---

## Step 7: Enable Custom Domain (Optional)

1. In Railway project, go to **Settings**
2. Under **Domains**, click **"Add Domain"**
3. Options:
   - **Railway subdomain** (free): `vehicleprofittracker.railway.app`
   - **Custom domain** (requires DNS setup): point your domain to Railway

---

## Troubleshooting

### Build Fails
- Check Railway Logs tab for errors
- Verify `.csproj` file is correct
- Ensure Dockerfile is present

### Database Connection Error
- Verify `DATABASE_URL` or `DefaultConnection` variable is set
- Test connection string locally first
- Check Railway PostgreSQL is running (green status)

### 500 Errors in Production
1. Click **Logs** in Railway dashboard
2. Look for error messages
3. Common issues:
   - Missing JWT Secret → Set `Jwt__Secret` variable
   - Database not initialized → Check migrations or manually run SQL
   - Missing CORS origins → Update `Cors__AllowedOrigins`

### Port Issues
- Railway automatically assigns port 8080
- Your Dockerfile already sets `ASPNETCORE_URLS=http://+:8080`
- No changes needed

---

## Environment Variables Mapping

Your `appsettings.json` uses this structure:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "..."
  },
  "Jwt": {
	"Secret": "...",
	"Issuer": "...",
	"Audience": "...",
	"AccessTokenExpiryMinutes": 60
  },
  "Cors": {
	"AllowedOrigins": ["..."]
  }
}
```

In Railway, set these as environment variables:
```
DefaultConnection=postgresql://...        (or use DATABASE_URL)
Jwt__Secret=your-secret
Jwt__Issuer=VehicleProfitTracker
Jwt__Audience=VehicleProfitTrackerClient
Jwt__AccessTokenExpiryMinutes=60
Cors__AllowedOrigins__0=https://frontend.com
```

---

## Production Security Checklist

- [ ] Change JWT Secret to a random 32+ character string
- [ ] Use Railway PostgreSQL or secure Supabase connection
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure CORS with specific frontend domain
- [ ] Enable HTTPS (Railway does this automatically)
- [ ] Review logs regularly for errors
- [ ] Set up error monitoring (Sentry, New Relic, etc.)

---

## Useful Railway CLI Commands

```powershell
# Install Railway CLI
npm install -g @railway/cli

# Login to Railway
railway login

# Check deployment status
railway status

# View logs
railway logs

# View variables
railway variables

# Deploy manually
railway deploy
```

---

## Next Steps

1. **Commit changes:** `git commit -am "Add Railway deployment config"`
2. **Push to GitHub:** `git push origin main`
3. **Monitor deployment** in Railway dashboard
4. **Share API URL** with your frontend team
5. **Configure frontend** to use the new Railway API URL

---

## Support

- Railway Docs: https://docs.railway.app
- GitHub Repo: https://github.com/RAJ432135/ProfitTrackerB
- .NET Docs: https://docs.microsoft.com/en-us/dotnet/

