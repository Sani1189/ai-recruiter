# Architecture Diagram - Kanban Board Implementation

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         FRONTEND (React/TypeScript)                  │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌──────────────────────────────┐    ┌──────────────────────────────┐
│  │   JobPostDetailsStep.tsx     │    │  KanbanBoardColumnsManager   │
│  │  (Enhanced with 5 new fields)│    │         (New Component)      │
│  │                              │    │                              │
│  │  - Industry                  │    │  - List columns              │
│  │  - Intro Text                │    │  - Create/Edit/Delete        │
│  │  - Requirements              │    │  - Toggle visibility         │
│  │  - What We Offer             │    │  - Reorder columns           │
│  │  - Company Info              │    │                              │
│  └──────────────────────────────┘    └──────────────────────────────┘
│                  │                                    │
│                  └────────────────┬───────────────────┘
│                                   ▼
│                    ┌────────────────────────┐
│                    │   JobsService.ts       │
│                    │   (Updated)            │
│                    │                        │
│                    │  New Methods:          │
│                    │  - getColumns()        │
│                    │  - createColumn()      │
│                    │  - moveJobToColumn()   │
│                    │  - getJobsByColumn()   │
│                    └────────────────────────┘
│                                   │
└───────────────────────────────────┼───────────────────────────────────
                                    │ HTTP/REST
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                       API GATEWAY / ROUTING                          │
├─────────────────────────────────────────────────────────────────────┤
│  POST   /api/KanbanBoardColumn/recruiter/{id}                       │
│  GET    /api/KanbanBoardColumn/recruiter/{id}                       │
│  PUT    /api/KanbanBoardColumn/{id}                                 │
│  DELETE /api/KanbanBoardColumn/{id}                                 │
│  PUT    /api/job/{name}/{version}/move-to-column/{columnId}         │
│  GET    /api/job/by-column/{recruiterId}                            │
└─────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    BACKEND CONTROLLERS (C#)                          │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌────────────────────────────────┐  ┌──────────────────────────────┐
│  │  KanbanBoardColumnController   │  │     JobController            │
│  │  (New)                         │  │     (Updated)                │
│  │                                │  │                              │
│  │  [Admin] GET /recruiter/{id}   │  │  [Admin] PUT  /move-to-col   │
│  │  [Admin] POST /recruiter/{id}  │  │  [Admin] GET  /by-column     │
│  │  [Admin] PUT /{id}             │  │                              │
│  │  [Admin] DELETE /{id}          │  │                              │
│  │  [Admin] POST /reorder         │  │                              │
│  └────────────────────────────────┘  └──────────────────────────────┘
│                  │                              │
│                  └──────────┬───────────────────┘
│                             ▼
│          ┌──────────────────────────────────┐
│          │    Service Layer (C#)            │
│          │                                  │
│          │  KanbanBoardColumnService        │
│          │  - GetColumnsAsync()             │
│          │  - CreateColumnAsync()           │
│          │  - UpdateColumnAsync()           │
│          │  - DeleteColumnAsync()           │
│          │  - ReorderColumnsAsync()         │
│          │                                  │
│          │  JobPostService (Updated)        │
│          │  - Enhanced with new fields      │
│          └──────────────────────────────────┘
│                             │
│                             ▼
│          ┌──────────────────────────────────┐
│          │    Entity Framework Core         │
│          │    RecruiterDbContext            │
│          │                                  │
│          │    DbSet<KanbanBoardColumn>      │
│          │    DbSet<JobPost> (Updated)      │
│          └──────────────────────────────────┘
│
└─────────────────────────────────────────────────────────────────────
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    DATABASE (SQL Server)                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌────────────────────────────────┐  ┌──────────────────────────────┐
│  │ UserProfiles (Existing)        │  │ KanbanBoardColumns (New)     │
│  ├────────────────────────────────┤  ├──────────────────────────────┤
│  │ UserProfileId (PK)             │  │ Id (PK)                      │
│  │ ...                            │  │ RecruiterId (FK) ────────┐   │
│  │                                │  │ ColumnName               │   │
│  └────────────────────────────────┘  │ Sequence                 │   │
│           ▲                           │ IsVisible                │   │
│           │                           │ CreatedAt/UpdatedAt      │   │
│           │ FK                        │ RowVersion (concurrency) │   │
│           │                           └─────┬────────────────────┘   │
│           │                                  │                       │
│           └──────────────────────────┬───────┘                       │
│                                      │                               │
│                                      │ One-to-Many                   │
│                                      ▼                               │
│           ┌────────────────────────────────────────────────────┐     │
│           │ JobPosts (Updated)                                 │     │
│           ├────────────────────────────────────────────────────┤     │
│           │ Name, Version (Composite PK)                       │     │
│           │ JobTitle                                           │     │
│           │ JobDescription                                     │     │
│           │ JobType                                            │     │
│           │ ExperienceLevel                                    │     │
│           │ MinimumRequirements                                │     │
│           │                                                    │     │
│           │ NEW FIELDS:                                        │     │
│           │ ├─ Industry (string, max 100)                      │     │
│           │ ├─ IntroText (string, max 500)                     │     │
│           │ ├─ Requirements (string, max)                      │     │
│           │ ├─ WhatWeOffer (string, max)                       │     │
│           │ ├─ CompanyInfo (string, max)                       │     │
│           │ └─ CurrentBoardColumnId (FK) ──────┐              │     │
│           │                                     │              │     │
│           │ Indexes:                            │              │     │
│           │ ├─ (Name, Version) - Unique         │              │     │
│           │ ├─ (CurrentBoardColumnId) - FK      │              │     │
│           │ └─ Other existing indexes...        │              │     │
│           └────────────────────────────────────┼──────────────┘     │
│                                                │                     │
│                            UNIQUE INDEX ──────┘                     │
│                            (RecruiterId, Sequence)                  │
│
└─────────────────────────────────────────────────────────────────────┘
```

---

## Data Flow Diagram

### 1. Create Kanban Column Flow

```
User clicks "Add Column"
    │
    ▼
Dialog opens (KanbanBoardColumnsManager)
    │
    ├─ User enters: "Column Name"
    │
    ▼
Form validation (Zod schema)
    │
    ├─ Required: Yes
    ├─ Min length: 1
    └─ Max length: 255
    │
    ▼
User clicks "Create"
    │
    ▼
POST /api/KanbanBoardColumn/recruiter/{recruiterId}
    │
    ├─ Headers: Authorization: Bearer {token}
    ├─ Body: { columnName: "Under Review", isVisible: true }
    │
    ▼
KanbanBoardColumnController.CreateColumn()
    │
    ├─ [Admin] authorization check
    ├─ Model validation
    │
    ▼
KanbanBoardColumnService.CreateColumnAsync()
    │
    ├─ Query existing columns
    ├─ Calculate next sequence (max + 1)
    ├─ Create new entity
    │
    ▼
DbContext.KanbanBoardColumns.AddAsync()
    │
    ├─ FK validation (RecruiterId exists in UserProfiles)
    ├─ Unique constraint check (RecruiterId, Sequence)
    │
    ▼
Database: INSERT into KanbanBoardColumns
    │
    ▼
201 Created + DTO returned
    │
    ▼
Frontend receives response
    │
    ├─ Add to columns list
    ├─ Re-sort by sequence
    ├─ Close dialog
    │
    ▼
UI shows new column in list
```

### 2. Move Job to Column Flow

```
User selects job + target column
    │
    ▼
PUT /api/job/{name}/{version}/move-to-column/{columnId}
    │
    ├─ Path params: name, version, columnId
    │
    ▼
JobController.MoveJobToColumn()
    │
    ├─ [Admin] authorization check
    ├─ Validate column exists
    │
    ▼
JobPostService.GetByIdAsync()
    │
    ├─ Query JobPost by (name, version)
    │
    ▼
Update JobPost.CurrentBoardColumnId = columnId
    │
    ▼
DbContext.SaveChangesAsync()
    │
    ├─ FK validation (CurrentBoardColumnId exists)
    │
    ▼
Database: UPDATE JobPosts SET CurrentBoardColumnId = @columnId
    │
    ▼
200 OK + Updated JobPost DTO returned
    │
    ▼
Frontend updates UI
    │
    ├─ Move job card to new column
    │
    ▼
Show success message
```

### 3. Get Jobs by Column Flow

```
User views kanban board
    │
    ▼
GET /api/job/by-column/{recruiterId}
    │
    ├─ Query params: page=1, pageSize=100
    │
    ▼
JobController.GetJobsByColumn()
    │
    ├─ [Admin] authorization check
    │
    ▼
JobPostService.GetListAsync()
    │
    ├─ Query JobPosts with pagination
    ├─ Filter by RecruiterId (optional)
    │
    ▼
LINQ/SQL: SELECT * FROM JobPosts WHERE ... ORDER BY CurrentBoardColumnId
    │
    ▼
Group by CurrentBoardColumnId in controller
    │
    ├─ Dictionary<Guid, List<JobPostDto>>
    │
    ▼
200 OK + Grouped data returned
    │
    ▼
Frontend receives data
    │
    ├─ Iterate through columns
    ├─ Display jobs in each column
    │
    ▼
Render kanban view with job cards
```

---

## Entity Relationship Diagram

```
┌─────────────────────┐
│   UserProfile       │
├─────────────────────┤
│ UserProfileId (PK)  │
│ Email               │
│ Name                │
│ ...                 │
└─────────────────────┘
         ▲
         │ 1:N
         │ (One recruiter : Many columns)
         │
    [FK] RecruiterId
         │
┌────────┴────────────────────────┐
│   KanbanBoardColumn             │
├─────────────────────────────────┤
│ Id (PK)                         │
│ RecruiterId (FK)                │
│ ColumnName                      │
│ Sequence                        │
│ IsVisible                       │
│ CreatedAt/UpdatedAt             │
├─────────────────────────────────┤
│ UNIQUE(RecruiterId, Sequence)   │
└────────────────────────────────┬┘
                                 │
                                 │ 1:N
                                 │ (One column : Many jobs)
                                 │
                            [FK] CurrentBoardColumnId
                                 │
                    ┌────────────┴─────────────┐
                    │                          │
                    ▼                          │ NULL
          ┌──────────────────┐                │
          │   JobPosts       │                │
          ├──────────────────┤                │
          │ Name, Version    │◄───────────────┘
          │   (Composite PK) │
          ├──────────────────┤
          │ JobTitle         │
          │ JobType          │
          │ JobDescription   │
          │ Industry      [NEW]
          │ IntroText     [NEW]
          │ Requirements  [NEW]
          │ WhatWeOffer   [NEW]
          │ CompanyInfo   [NEW]
          │ CurrentBoard
          │ ColumnId      [NEW]
          │ Status           │
          │ ...              │
          └──────────────────┘
```

---

## Component Hierarchy

```
App
├── Layout
│   └── RecruiterLayout
│       └── Jobs Page
│           ├── Header
│           ├── KanbanBoardColumnsManager (New)
│           │   ├── Column List
│           │   │   ├── Column Item
│           │   │   │   ├── Eye Icon (Visibility Toggle)
│           │   │   │   ├── Edit Button
│           │   │   │   └── Delete Button
│           │   │   └── Empty State
│           │   ├── Add Column Button
│           │   └── Dialog
│           │       ├── Column Name Input
│           │       └── Create/Update Button
│           │
│           └── JobPostForm (Updated)
│               ├── JobPostDetailsStep (Updated)
│               │   ├── Existing Fields
│               │   │   ├── Status
│               │   │   ├── Origin Country
│               │   │   ├── Job Title
│               │   │   ├── Job Type
│               │   │   └── ...
│               │   │
│               │   └── New Fields
│               │       ├── Industry (Input)
│               │       ├── Intro Text (Input)
│               │       ├── Requirements (Textarea)
│               │       ├── What We Offer (Textarea)
│               │       └── Company Info (Textarea)
│               │
│               ├── JobStepsStep
│               └── ConfirmationStep
```

---

## Database Schema Visualization

```
CREATE TABLE KanbanBoardColumns (
    Id                UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RecruiterId       UNIQUEIDENTIFIER NOT NULL,
    ColumnName        NVARCHAR(255) NOT NULL,
    Sequence          INT NOT NULL,
    IsVisible         BIT NOT NULL DEFAULT 1,
    CreatedAt         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RowVersion        ROWVERSION,
    
    CONSTRAINT FK_KanbanBoardColumns_UserProfiles 
        FOREIGN KEY (RecruiterId) REFERENCES UserProfiles(UserProfileId) 
        ON DELETE CASCADE,
    
    CONSTRAINT UQ_KanbanBoardColumns_RecruiterId_Sequence 
        UNIQUE (RecruiterId, Sequence),
    
    INDEX IX_KanbanBoardColumns_RecruiterId 
        ON RecruiterId
);

ALTER TABLE JobPosts ADD (
    Industry                NVARCHAR(100) NOT NULL DEFAULT '',
    IntroText              NVARCHAR(500) NOT NULL DEFAULT '',
    Requirements           NVARCHAR(MAX) NOT NULL DEFAULT '',
    WhatWeOffer            NVARCHAR(MAX) NOT NULL DEFAULT '',
    CompanyInfo            NVARCHAR(MAX) NOT NULL DEFAULT '',
    CurrentBoardColumnId   UNIQUEIDENTIFIER NULL,
    
    CONSTRAINT FK_JobPosts_KanbanBoardColumns_CurrentBoardColumnId
        FOREIGN KEY (CurrentBoardColumnId) 
        REFERENCES KanbanBoardColumns(Id) 
        ON DELETE SET NULL,
    
    INDEX IX_JobPosts_CurrentBoardColumnId 
        ON CurrentBoardColumnId
);
```

---

## Request/Response Examples

### Create Column Request/Response

```
REQUEST:
POST /api/KanbanBoardColumn/recruiter/recruiter-uuid-123
Authorization: Bearer token
Content-Type: application/json

{
  "columnName": "In Review",
  "isVisible": true
}

RESPONSE (201 Created):
{
  "id": "column-uuid-456",
  "recruiterId": "recruiter-uuid-123",
  "columnName": "In Review",
  "sequence": 2,
  "isVisible": true,
  "createdAt": "2026-02-10T10:30:00Z",
  "updatedAt": "2026-02-10T10:30:00Z"
}
```

### Create Job with New Fields Request/Response

```
REQUEST:
POST /api/job
Authorization: Bearer token
Content-Type: application/json

{
  "name": "senior-dev-2026",
  "jobTitle": "Senior Developer",
  "jobType": "FullTime",
  "experienceLevel": "Senior",
  "jobDescription": "We seek an experienced developer...",
  "industry": "Technology",
  "introText": "Join our innovative team",
  "requirements": "10+ years experience with TypeScript, React...",
  "whatWeOffer": "Competitive salary, remote, health insurance...",
  "companyInfo": "Leading AI company focused on recruitment...",
  "minimumRequirements": ["TypeScript", "React"],
  "maxAmountOfCandidatesRestriction": 100,
  "status": "Draft"
}

RESPONSE (201 Created):
{
  "name": "senior-dev-2026",
  "version": 1,
  "jobTitle": "Senior Developer",
  "jobType": "FullTime",
  "experienceLevel": "Senior",
  "jobDescription": "We seek an experienced developer...",
  "industry": "Technology",
  "introText": "Join our innovative team",
  "requirements": "10+ years experience with TypeScript, React...",
  "whatWeOffer": "Competitive salary, remote, health insurance...",
  "companyInfo": "Leading AI company focused on recruitment...",
  "currentBoardColumnId": null,
  "status": "Draft",
  "createdAt": "2026-02-10T10:30:00Z",
  "updatedAt": "2026-02-10T10:30:00Z"
}
```

---

## Validation Rules Matrix

```
Field                   | Type      | Required | Length/Range      | Validation
─────────────────────────────────────────────────────────────────────────────────
ColumnName              | string    | Yes      | 1-255             | NOT NULL
Sequence                | integer   | Yes      | 1-N               | AUTO
IsVisible               | boolean   | Yes      | true/false        | Default true
RecruiterId (Column)    | UUID      | Yes      | Valid UUID        | FK to UserProfile
─────────────────────────────────────────────────────────────────────────────────
Industry                | string    | Yes      | 2-100             | NOT NULL
IntroText              | string    | Yes      | 10-500            | NOT NULL
Requirements           | string    | Yes      | Min 10            | NOT NULL
WhatWeOffer            | string    | Yes      | Min 10            | NOT NULL
CompanyInfo            | string    | Yes      | Min 10            | NOT NULL
CurrentBoardColumnId   | UUID      | No       | Valid UUID/NULL   | FK to Column
```

---

## Performance Considerations

```
Query: Get all columns for recruiter
SQL: SELECT * FROM KanbanBoardColumns 
     WHERE RecruiterId = @recruiterId 
     ORDER BY Sequence

Index: IX_KanbanBoardColumns_RecruiterId
Result: ~O(log n) lookup time

Query: Get jobs by column
SQL: SELECT * FROM JobPosts 
     WHERE CurrentBoardColumnId = @columnId

Index: IX_JobPosts_CurrentBoardColumnId
Result: ~O(log n) lookup time

Query: Get jobs by recruiter
SQL: SELECT * FROM JobPosts jp
     INNER JOIN KanbanBoardColumns kbc 
     ON jp.CurrentBoardColumnId = kbc.Id
     WHERE kbc.RecruiterId = @recruiterId

Index: Multiple on both FKs
Result: ~O(n) with efficient joins
```

---

## Summary

This architecture provides:
- ✓ Clean separation of concerns
- ✓ Type-safe frontend-backend communication
- ✓ Proper database constraints and relationships
- ✓ Scalable design for future enhancements
- ✓ Performance optimized with proper indexes
- ✓ Complete data flow from UI to database
