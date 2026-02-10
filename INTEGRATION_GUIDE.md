# Integration & Testing Guide

## Quick Start: Running the Application

### 1. Database Setup
```bash
# Apply the migration to your database
cd ai/CSharpSolutions/Recruiter
dotnet ef database update

# Or with specific migration
dotnet ef database update AddKanbanBoardColumnsAndJobPostFields
```

### 2. Verify Database Changes
```sql
-- Check new table exists
SELECT * FROM KanbanBoardColumns;

-- Check JobPosts columns
EXEC sp_columns @table_name = 'JobPosts';

-- Verify indexes
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('KanbanBoardColumns');
```

---

## API Testing Guide

### 1. Get Kanban Columns for Recruiter
```bash
curl -X GET "http://localhost:5000/api/KanbanBoardColumn/recruiter/{recruiterId}" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json"
```

**Expected Response** (200 OK):
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "recruiterId": "5fa85f64-5717-4562-b3fc-2c963f66afa7",
    "columnName": "Draft",
    "sequence": 1,
    "isVisible": true,
    "createdAt": "2026-02-10T10:00:00Z",
    "updatedAt": "2026-02-10T10:00:00Z"
  }
]
```

### 2. Create Kanban Column
```bash
curl -X POST "http://localhost:5000/api/KanbanBoardColumn/recruiter/{recruiterId}" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "columnName": "In Review",
    "isVisible": true
  }'
```

**Expected Response** (201 Created):
```json
{
  "id": "new-uuid-here",
  "recruiterId": "recruiter-id",
  "columnName": "In Review",
  "sequence": 2,
  "isVisible": true,
  "createdAt": "2026-02-10T10:00:00Z",
  "updatedAt": "2026-02-10T10:00:00Z"
}
```

### 3. Create Job Post with New Fields
```bash
curl -X POST "http://localhost:5000/api/job" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "senior-developer-2026",
    "jobTitle": "Senior Full Stack Developer",
    "jobType": "FullTime",
    "experienceLevel": "Senior",
    "jobDescription": "We are looking for an experienced developer...",
    "industry": "Technology",
    "introText": "Join our innovative tech team building AI solutions",
    "requirements": "10+ years of experience with TypeScript, React, Node.js...",
    "whatWeOffer": "Competitive salary, remote work, health insurance...",
    "companyInfo": "We are a leading AI company focused on recruitment...",
    "minimumRequirements": ["TypeScript", "React", "Node.js"],
    "maxAmountOfCandidatesRestriction": 100,
    "status": "Draft"
  }'
```

### 4. Move Job to Column
```bash
curl -X PUT "http://localhost:5000/api/job/{name}/{version}/move-to-column/{columnId}" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json"
```

**Expected Response** (200 OK):
```json
{
  "name": "senior-developer-2026",
  "version": 1,
  "currentBoardColumnId": "column-id",
  "status": "Draft",
  // ... other fields
}
```

### 5. Get Jobs by Column
```bash
curl -X GET "http://localhost:5000/api/job/by-column/{recruiterId}" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json"
```

**Expected Response** (200 OK):
```json
{
  "00000000-0000-0000-0000-000000000000": [
    { "name": "job1", "version": 1, "status": "Draft" }
  ],
  "column-uuid-1": [
    { "name": "job2", "version": 1, "status": "Draft" }
  ]
}
```

---

## Frontend Testing Scenarios

### Scenario 1: Create New Job Post with New Fields
1. Navigate to **Jobs > New**
2. Fill out Job Post Details step:
   - Job Title: "Senior React Developer"
   - Job Type: "FullTime"
   - Experience Level: "Senior"
   - Job Description: (detailed description)
   - Industry: "Technology"
   - Intro Text: "Brief introduction"
   - Requirements: "Detailed requirements"
   - What We Offer: "Benefits and compensation"
   - Company Info: "Company overview"
3. Click **Next** to proceed to Job Steps
4. Click **Next** to Confirmation
5. Click **Create** to submit

**Expected Result**: Job post created with all new fields stored in database

### Scenario 2: Manage Kanban Columns
1. Navigate to **Jobs Dashboard**
2. Look for "Kanban Board Columns" section
3. Click **Add Column**
4. Enter column name: "Under Review"
5. Click **Create**

**Expected Result**: New column appears in list with sequence number 3

### Scenario 3: Toggle Column Visibility
1. In Kanban Board Columns section
2. Find column "Under Review"
3. Click eye icon to hide
4. Verify column shows as hidden

**Expected Result**: Column is-visible flag toggles in database

### Scenario 4: Edit Job Posting
1. Navigate to existing job post
2. Edit form pre-populates with:
   - Industry: (previous value)
   - Intro Text: (previous value)
   - Requirements: (previous value)
   - What We Offer: (previous value)
   - Company Info: (previous value)
3. Modify any fields
4. Click **Update**

**Expected Result**: Changes saved, version incremented, new fields persisted

---

## Database Verification Queries

### Verify Migration Applied
```sql
SELECT name FROM sys.tables WHERE name = 'KanbanBoardColumns';
-- Should return: KanbanBoardColumns
```

### Check JobPost New Columns
```sql
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'JobPosts' 
AND COLUMN_NAME IN ('Industry', 'IntroText', 'Requirements', 'WhatWeOffer', 'CompanyInfo', 'CurrentBoardColumnId');
-- Should return 6 rows
```

### Verify Unique Index
```sql
SELECT * FROM sys.indexes 
WHERE object_id = OBJECT_ID('KanbanBoardColumns') 
AND name LIKE '%RecruiterId%Sequence%';
-- Should show unique index on (RecruiterId, Sequence)
```

### Verify Foreign Key Relationships
```sql
SELECT * FROM sys.foreign_keys 
WHERE parent_object_id = OBJECT_ID('KanbanBoardColumns') 
OR referenced_object_id = OBJECT_ID('KanbanBoardColumns');
-- Should show FK relationships to UserProfiles and JobPosts
```

### Get All Columns for a Recruiter
```sql
SELECT Id, ColumnName, Sequence, IsVisible 
FROM KanbanBoardColumns 
WHERE RecruiterId = '{recruiterId}'
ORDER BY Sequence;
```

### Get All Jobs in a Specific Column
```sql
SELECT Name, Version, CurrentBoardColumnId 
FROM JobPosts 
WHERE CurrentBoardColumnId = '{columnId}';
```

---

## Error Handling & Troubleshooting

### Common Issues

#### 1. Migration Fails
**Error**: "Invalid column name" or "Timeout expired"

**Solutions**:
- Ensure database connection string is correct
- Check if database is accessible
- Try: `dotnet ef migrations remove` and reapply
- Check SQL Server permissions

#### 2. Foreign Key Constraint Fails
**Error**: "The INSERT, UPDATE, or DELETE statement conflicted with a FOREIGN KEY constraint"

**Solution**: 
- Verify RecruiterId exists in UserProfiles table
- Check CurrentBoardColumnId exists in KanbanBoardColumns table

#### 3. Unique Constraint Violation
**Error**: "Cannot insert duplicate key row in object 'KanbanBoardColumns' with unique index"

**Solution**: 
- Verify sequence numbers are unique per recruiter
- Check if trying to create duplicate column for same sequence

#### 4. Frontend Form Validation Fails
**Error**: Red validation messages below fields

**Solutions**:
- Ensure all required fields are filled
- Check field length constraints:
  - Industry: 2-100 chars
  - IntroText: 10-500 chars
  - Requirements: min 10 chars
  - WhatWeOffer: min 10 chars
  - CompanyInfo: min 10 chars

---

## Performance Testing

### Load Test: Bulk Column Creation
```csharp
// Create 100 columns for a recruiter
for (int i = 0; i < 100; i++)
{
    await jobsService.createColumn(recruiterId, {
        columnName: `Column ${i}`,
        isVisible: true
    });
}
// Expected: All succeed with proper sequencing
```

### Query Performance: Get Jobs by Column
```csharp
// Get all jobs grouped by column
var jobs = await jobsService.getJobsByColumn(recruiterId, 1, 1000);
// Expected: Returns within 500ms
```

---

## Security Verification

### 1. Authorization Check
All endpoints should return **401 Unauthorized** when called without token:
```bash
curl -X GET "http://localhost:5000/api/KanbanBoardColumn/recruiter/{recruiterId}"
# Should return 401
```

### 2. Admin Policy Check
All endpoints should return **403 Forbidden** for non-admin users:
```bash
curl -X POST "http://localhost:5000/api/KanbanBoardColumn/recruiter/{recruiterId}" \
  -H "Authorization: Bearer {user-token-not-admin}"
# Should return 403
```

### 3. Cross-Tenant Isolation
User from Tenant A should not access Tenant B's columns:
```bash
# This requires testing in multi-tenant environment
# Verify TenantId filtering in queries
```

---

## Rollback Procedures

### If Migration Causes Issues
```bash
# Remove migration
dotnet ef migrations remove

# Or revert to previous version
dotnet ef database update {PreviousMigration}
```

### Manual Rollback SQL
```sql
-- If migration fails, manually drop columns
ALTER TABLE JobPosts DROP CONSTRAINT FK_JobPosts_KanbanBoardColumns_CurrentBoardColumnId;
ALTER TABLE JobPosts DROP COLUMN Industry;
ALTER TABLE JobPosts DROP COLUMN IntroText;
ALTER TABLE JobPosts DROP COLUMN Requirements;
ALTER TABLE JobPosts DROP COLUMN WhatWeOffer;
ALTER TABLE JobPosts DROP COLUMN CompanyInfo;
ALTER TABLE JobPosts DROP COLUMN CurrentBoardColumnId;
DROP TABLE KanbanBoardColumns;
```

---

## Documentation Links

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Zod Validation Library](https://zod.dev/)
- [React Hook Form](https://react-hook-form.com/)
- [Next.js API Routes](https://nextjs.org/docs/api-routes/introduction)

---

## Support & Questions

For issues or questions:
1. Check error logs in `bin/` directory
2. Review database constraints and indexes
3. Verify API token and permissions
4. Check frontend console for TypeScript errors
5. Review implementation summary for detailed structure

---

## Version Information

- **Migration Date**: 2026-02-10
- **Backend Version**: .NET 8.0+
- **Frontend Version**: Next.js 15+, React 19+
- **Database**: SQL Server 2019+

---

## Checklist for Production Deployment

- [ ] Database backup taken
- [ ] Migration tested in staging
- [ ] All endpoints tested with Postman/Insomnia
- [ ] Frontend components render without errors
- [ ] Form validation working correctly
- [ ] Error messages user-friendly
- [ ] Authorization checks passing
- [ ] Performance acceptable (< 500ms queries)
- [ ] Logs reviewed for warnings
- [ ] Rollback procedure documented and tested
- [ ] User documentation updated
- [ ] Stakeholders notified

---

## Success Criteria

✓ Database migration applies successfully
✓ All API endpoints respond with correct status codes
✓ Frontend forms display new fields and validate correctly
✓ Kanban columns can be created, updated, deleted
✓ Jobs can be moved between columns
✓ All new job posting fields persist to database
✓ No performance degradation
✓ Proper error handling and user feedback
✓ Security policies enforced
✓ Backward compatibility maintained
