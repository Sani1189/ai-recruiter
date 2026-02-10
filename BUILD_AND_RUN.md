# Build and Run Instructions

## System Requirements
- .NET 8.0 or higher
- Node.js 18+ and npm
- SQL Server or compatible database
- Git (for version control)

---

## Step 1: Apply Database Migration

```bash
# Navigate to the backend project
cd ai/CSharpSolutions/Recruiter

# Apply the migration to your database
dotnet ef database update --project Infrastructure --startup-project WebApi
```

**What this does:**
- Creates `KanbanBoardColumns` table
- Adds 6 new columns to `JobPosts` table
- Creates relationships and indexes
- Initializes all constraints

**If successful, you'll see:**
```
Done. Building...
Done.
Applying migration '20260210000000_AddKanbanBoardColumnsAndJobPostFields'.
Done.
```

---

## Step 2: Build Backend

```bash
# From ai/CSharpSolutions/Recruiter directory

# Clean build (recommended)
dotnet clean
dotnet build

# Or quick build
dotnet build --no-restore
```

**Expected output:**
```
Microsoft (R) Build Engine version 17.x.x
Build started 1/1/2024 12:00:00 PM
...
Build succeeded. 0 Warning(s)
```

**If you get errors:**
- Check connection string in `appsettings.json`
- Verify database server is running
- Ensure migration was applied successfully

---

## Step 3: Run Backend

```bash
# From ai/CSharpSolutions/Recruiter directory

dotnet run --project WebApi
```

**Expected output:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

**Backend is now running at:**
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger/index.html`

**Keep this terminal open!**

---

## Step 4: Setup Frontend (New Terminal)

Open a new terminal window/tab

```bash
# Navigate to frontend directory
cd ai/Frontend

# Install dependencies (do this once)
npm install

# Verify dependencies installed
npm list --depth=0
```

**Expected output:**
```
up to date in X.XXs
```

---

## Step 5: Run Frontend

```bash
# From ai/Frontend directory

npm run dev
```

**Expected output:**
```
> dev
> next dev

▲ Next.js 14.x.x
- Local:        http://localhost:3000
- Environments: .env.local

✓ Ready in XXXX ms
```

**Frontend is now running at:**
- `http://localhost:3000`

**Open in browser:** http://localhost:3000

---

## Verification Checklist

### Backend Verification
```bash
# Test the API is running
curl https://localhost:5001/swagger/index.html

# Or check health endpoint
curl https://localhost:5001/health
```

### Frontend Verification
- [ ] Page loads in browser
- [ ] No console errors
- [ ] Forms render correctly
- [ ] Network tab shows requests to backend

### Database Verification
```sql
-- Check tables exist
SELECT name FROM sys.tables WHERE name IN ('KanbanBoardColumns', 'JobPosts')

-- Check new columns in JobPosts
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'JobPosts' 
AND COLUMN_NAME IN ('Industry', 'IntroText', 'Requirements', 'WhatWeOffer', 'CompanyInfo', 'CurrentBoardColumnId')

-- Should return 6 rows
```

---

## Testing the Implementation

### Test 1: Create a Kanban Column

```bash
# First, get a recruiter ID from your database or login flow
# Then run:

curl -X POST https://localhost:5001/KanbanBoardColumn/recruiter/{recruiterId} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {your_token}" \
  -d '{
    "columnName": "Active Positions",
    "isVisible": true
  }'

# Expected response:
# {
#   "id": "...",
#   "recruiterId": "...",
#   "columnName": "Active Positions",
#   "sequence": 1,
#   "isVisible": true
# }
```

### Test 2: List Kanban Columns

```bash
curl https://localhost:5001/KanbanBoardColumn/recruiter/{recruiterId} \
  -H "Authorization: Bearer {your_token}"

# Expected response:
# [
#   {
#     "id": "...",
#     "columnName": "Active Positions",
#     "sequence": 1,
#     "isVisible": true
#   }
# ]
```

### Test 3: Update Column

```bash
curl -X PUT https://localhost:5001/KanbanBoardColumn/{columnId} \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {your_token}" \
  -d '{
    "columnName": "Closed Positions",
    "isVisible": false
  }'
```

### Test 4: Create Job with New Fields

Use the frontend form to create a job post with the new fields:
- Industry
- Intro Text
- Requirements
- What We Offer
- Company Info

---

## Troubleshooting

### Backend Won't Start

**Error:** `"Unhandled exception. System.Data.SqlClient.SqlException"`
- **Fix:** Check connection string in `appsettings.json`
- Verify SQL Server is running
- Test connection: `sqlcmd -S localhost -U sa -P {password}`

**Error:** `"It was not possible to find any compatible framework version"`
- **Fix:** Install .NET 8: https://dotnet.microsoft.com/download
- Verify: `dotnet --version`

### Frontend Won't Start

**Error:** `Cannot find module 'next'`
- **Fix:** Run `npm install` in ai/Frontend directory
- Clear cache: `rm -rf node_modules && npm install`

**Error:** `EADDRINUSE: address already in use :::3000`
- **Fix:** Kill process on port 3000
  - Linux/Mac: `kill -9 $(lsof -t -i:3000)`
  - Windows: `netstat -ano | findstr :3000` then `taskkill /PID {PID} /F`

### Database Migration Failed

**Error:** `"There is already an object named 'KanbanBoardColumns'"`
- **Fix:** Migration already applied
- Check: `dotnet ef migrations list`

**Error:** `"Login failed for user 'sa'"`
- **Fix:** Check SQL Server credentials
- Verify: `sqlcmd -S localhost -U sa -P {password} -Q "SELECT 1"`

### API Calls Return 401

**Error:** `"Unauthorized"`
- **Fix:** Add Authorization header with valid token
- Get token from login endpoint first
- Format: `Authorization: Bearer {token}`

---

## Common Commands Reference

```bash
# Backend
cd ai/CSharpSolutions/Recruiter
dotnet build              # Build backend
dotnet run --project WebApi   # Run backend
dotnet ef database update      # Apply migration
dotnet ef migrations add {name}  # Create migration

# Frontend
cd ai/Frontend
npm install              # Install dependencies
npm run dev             # Run development server
npm run build           # Build for production
npm run start           # Start production server
npm run lint            # Check code quality

# Database
dotnet ef database update      # Apply pending migrations
dotnet ef migrations list      # Show all migrations
dotnet ef migrations remove    # Remove last migration
dotnet ef migrations script    # Generate SQL script
```

---

## Port Configuration

If you need to change ports:

### Backend Port
Edit: `ai/CSharpSolutions/Recruiter/WebApi/Properties/launchSettings.json`
```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:5001;http://localhost:5000"
    }
  }
}
```

### Frontend Port
```bash
npm run dev -- -p 3001
# Or set environment variable
# PORT=3001 npm run dev
```

---

## Development Workflow

```bash
# Terminal 1: Backend
cd ai/CSharpSolutions/Recruiter
dotnet run --project WebApi
# Stays running

# Terminal 2: Frontend  
cd ai/Frontend
npm run dev
# Stays running

# Terminal 3: Development/Testing
# Make code changes
# Changes auto-reload in both backend and frontend
```

---

## Production Build

### Backend
```bash
dotnet publish -c Release -o ./publish
# Output in ./publish/WebApi.dll
```

### Frontend
```bash
npm run build
npm run start
# Optimized production build
```

---

## Environment Variables

### Backend (.env or appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=recruiter;User Id=sa;Password=..."
  },
  "Jwt": {
    "Key": "your-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
  }
}
```

### Frontend (.env.local)
```
NEXT_PUBLIC_API_URL=https://localhost:5001
NEXT_PUBLIC_APP_NAME=AI Recruiter
```

---

## Performance Tips

1. **Use HTTPS** - Backend uses HTTPS by default
2. **Enable Caching** - Frontend caches API responses with SWR
3. **Database Indexes** - Already created on (RecruiterId, Sequence)
4. **Pagination** - Use for large job lists
5. **Compression** - Both frontend and backend support gzip

---

## Success Indicators

✅ Backend running without errors  
✅ Frontend loads in browser  
✅ Network requests show 200 status  
✅ No console errors  
✅ Database tables exist  
✅ API endpoints respond  
✅ Forms accept input  
✅ Data persists to database  

---

## Next Steps After Setup

1. **Login** - Create/login with a recruiter account
2. **Create Column** - Use the Kanban manager to add columns
3. **Create Job** - Post a new job with all the new fields
4. **Move Job** - Drag job to different columns
5. **Test API** - Use Postman to test endpoints

---

## Getting Help

If you encounter issues:

1. Check `BUILD_FIXES_APPLIED.md` for error explanations
2. Check `INTEGRATION_GUIDE.md` for API examples
3. Check logs in browser console (Frontend)
4. Check application output (Backend)
5. Check SQL Server error logs (Database)

---

## Important Files to Keep Updated

- `appsettings.json` - Database connection
- `.env.local` - Frontend environment variables
- Database backups - Before applying migrations
- Migration files - Never modify manually

---

## Final Checklist

Before declaring setup complete:

- [ ] .NET SDK installed and verified
- [ ] SQL Server running
- [ ] Node.js and npm installed
- [ ] Migration applied successfully
- [ ] Backend builds without errors
- [ ] Backend runs without crashes
- [ ] Frontend npm install completed
- [ ] Frontend runs without errors
- [ ] Browser opens successfully
- [ ] API responds to requests
- [ ] Database tables verified
- [ ] No console errors

**When all items are checked, your implementation is ready!**
