# Questionnaire Template Versioning Workflow

## Overview
This document describes the complete workflow for questionnaire template operations, including editing, versioning, and concurrency handling.

---

## 1. Template Update Request Flow

### Entry Point: `QuestionnaireTemplateService.UpdateAsync()`

```
Frontend Request (PUT /QuestionnaireTemplate/{name}/{version})
    ↓
QuestionnaireTemplateService.UpdateAsync()
    ↓
[Validation] FluentValidation checks DTO
    ↓
[Check Template Usage] IsTemplateInUseAsync()
    ↓
[Decision Point] Determine action based on:
    - isInUse (has submissions?)
    - dto.ShouldUpdateVersion (user requested template versioning?)
```

---

## 2. Decision Tree: Template Update Flow

### Scenario A: Template NOT in Use (No Submissions)

```
┌─────────────────────────────────────────┐
│ Template NOT in Use                    │
└─────────────────────────────────────────┘
    │
    ├─→ [Direct Edit Allowed]
    │   │
    │   ├─→ Edit Template Properties
    │   │   (Title, Description, TimeLimit, etc.)
    │   │
    │   ├─→ SyncSectionsAsync()
    │   │   │
    │   │   ├─→ Add/Edit/Delete Sections ✅
    │   │   │
    │   │   ├─→ SyncQuestionsAsync()
    │   │   │   │
    │   │   │   ├─→ Add/Edit/Delete Questions ✅
    │   │   │   │
    │   │   │   ├─→ SyncOptionsAsync()
    │   │   │   │   │
    │   │   │   │   ├─→ Add/Edit/Delete Options ✅
    │   │   │   │   │
    │   │   │   │   └─→ [If template is in use AND option/question changes]
    │   │   │   │       └─→ Auto-version question (preserve history)
    │   │   │
    │   │   └─→ SaveChangesAsync() → Success
    │   │
    │   └─→ Return Updated Template
```

### Scenario B: Template IN Use (Has Submissions)

```
┌─────────────────────────────────────────┐
│ Template IN Use (Has Submissions)       │
└─────────────────────────────────────────┘
    │
    ├─→ [Check User Intent]
    │   │
    │   ├─→ [If ShouldUpdateVersion = false]
    │   │   └─→ ✅ Proceed: auto-version changed questions when needed
    │   │
    │   └─→ [If ShouldUpdateVersion = true]
    │       └─→ ✅ PROCEED TO TEMPLATE VERSIONING
    │
    └─→ VersionTemplateAsync()
        │
        ├─→ [Load Source Template] (NoTracking)
        │   └─→ QuestionnaireTemplateByNameAndVersionNoTrackingSpec
        │
        ├─→ [Retry Loop: Max 5 Attempts]
        │   │
        │   ├─→ VersionTemplateInternalAsync()
        │   │   │
        │   │   ├─→ CalculateNextTemplateVersionAsync()
        │   │   │   └─→ Fetch latest version (NoTracking)
        │   │   │       └─→ nextVersion = (latest?.Version ?? current) + 1
        │   │   │
        │   │   ├─→ [Check if version already exists]
        │   │   │   └─→ If exists → Return existing version
        │   │   │
        │   │   ├─→ CreateNewTemplateEntity()
        │   │   │   └─→ New template with nextVersion
        │   │   │
        │   │   ├─→ [For each Section in DTO]
        │   │   │   │
        │   │   │   ├─→ VersionSectionAsync()
        │   │   │   │   │
        │   │   │   │   ├─→ Create new Section (new Guid)
        │   │   │   │   │
        │   │   │   │   ├─→ [For each Question in Section]
        │   │   │   │   │   │
        │   │   │   │   │   ├─→ VersionOrCreateQuestionAsync()
        │   │   │   │   │   │   │
        │   │   │   │   │   │   ├─→ [If old question exists]
        │   │   │   │   │   │   │   └─→ VersionQuestionAsync()
        │   │   │   │   │   │   │       └─→ Calculate next question version
        │   │   │   │   │   │   │
        │   │   │   │   │   │   ├─→ [If new question]
        │   │   │   │   │   │   │   └─→ CreateNewQuestion() (Version = 1)
        │   │   │   │   │   │   │
        │   │   │   │   │   │   ├─→ AddAsync(newQuestion)
        │   │   │   │   │   │   │
        │   │   │   │   │   │   └─→ [For each Option]
        │   │   │   │   │   │       │
        │   │   │   │   │   │       ├─→ [If old option exists]
        │   │   │   │   │   │       │   └─→ VersionOptionAsync()
        │   │   │   │   │   │       │       └─→ Calculate next option version
        │   │   │   │   │   │       │
        │   │   │   │   │   │       ├─→ [If new option]
        │   │   │   │   │   │       │   └─→ CreateNewOption() (Version = 1)
        │   │   │   │   │   │       │
        │   │   │   │   │   │       └─→ AddAsync(newOption)
        │   │   │   │   │   │
        │   │   │   │   │   └─→ Add question to section
        │   │   │   │   │
        │   │   │   │   └─→ Add section to template
        │   │   │   │
        │   │   │   └─→ AddAsync(newTemplate)
        │   │   │
        │   │   └─→ SaveChangesAsync()
        │   │       │
        │   │       ├─→ [Repository Layer: QuestionnaireTemplateRepository]
        │   │       │   │
        │   │       │   ├─→ [If DbUpdateConcurrencyException]
        │   │       │   │   └─→ Retry up to 3 times
        │   │       │   │       └─→ Refresh RowVersion from DB
        │   │       │   │
        │   │       │   └─→ [If Unique Constraint Violation]
        │   │       │       └─→ Bubble up to Orchestrator
        │   │       │
        │   │       └─→ [If Unique Constraint Violation in Orchestrator]
        │   │           └─→ Retry with exponential backoff
        │   │               └─→ Recalculate version
        │   │
        │   └─→ [On Success] Return new template version
        │
        └─→ [On Failure] Retry or throw exception
```

---

## 3. Question-Level Versioning Flow

### When User Edits a Question (Template IN Use)

```
┌─────────────────────────────────────────┐
│ User Edits Question                    │
│ (Template has submissions)              │
└─────────────────────────────────────────┘
    │
    ├─→ SyncQuestionsAsync()
    │   │
    │   ├─→ [Check: Template is in use AND question changed?]
    │   │   └─→ YES → Auto-version question
    │   │       │
    │   │       ├─→ [Load Template] (NoTracking)
    │   │       │
    │   │       ├─→ [Retry Loop: Max 5 Attempts]
    │   │       │   │
    │   │       │   ├─→ VersionTemplateForQuestionInternalAsync()
    │   │       │   │   │
    │   │       │   │   ├─→ CalculateNextTemplateVersionAsync()
    │   │       │   │   │
    │   │       │   │   ├─→ CreateNewTemplateEntityFromExisting()
    │   │       │   │   │
    │   │       │   │   ├─→ [For each Section]
    │   │       │   │   │   │
    │   │       │   │   │   └─→ VersionSectionForQuestionEditAsync()
    │   │       │   │   │       │
    │   │       │   │   │       ├─→ [For each Question]
    │   │       │   │   │       │   │
    │   │       │   │   │       │   ├─→ [If edited question]
    │   │       │   │   │       │   │   └─→ Version with changes
    │   │       │   │   │       │   │
    │   │       │   │   │       │   └─→ [If other question]
    │   │       │   │   │       │       └─→ Version without changes
    │   │       │   │   │       │
    │   │       │   │   │       └─→ Version all options
    │   │       │   │   │
    │   │       │   │   └─→ SaveChangesAsync()
    │   │       │   │
    │   │       │   └─→ Return new template version
    │   │       │
    │   │       └─→ [On Conflict] Retry with backoff
    │   │
    │   ├─→ [Check: Question Changed?]
    │   │   └─→ YES → [If template in use]
    │   │       └─→ ❌ THROW ERROR: "Please check 'Version template also'"
    │   │
    │   └─→ [If template NOT in use]
    │       └─→ VersionQuestionAsync()
    │           └─→ Version question + all options
    │               └─→ Replace in section
```

### When User Edits an Option (Template IN Use)

```
┌─────────────────────────────────────────┐
│ User Edits Option                       │
│ (Template has submissions)              │
└─────────────────────────────────────────┘
    │
    ├─→ SyncOptionsAsync()
    │   │
    │   ├─→ [Check: Template is in use AND option changed?]
    │   │   └─→ YES → Auto-version question (includes all options)
    │   │       │
    │   │       ├─→ VersionQuestionAsync()
    │   │       │
    │   │       ├─→ [For each Option]
    │   │       │   │
    │   │       │   ├─→ [If edited option]
    │   │       │   │   └─→ Apply changes
    │   │       │   │
    │   │       │   └─→ [If other option]
    │   │       │       └─→ Version without changes
    │   │       │
    │   │       └─→ Replace question in section
    │   │
    │   ├─→ [Check: Option Changed?]
    │   │   └─→ YES → [If template in use]
    │   │       └─→ ❌ THROW ERROR: "Please check 'Version question also'"
    │   │
    │   └─→ [If template NOT in use]
    │       └─→ VersionOptionAsync()
    │           └─→ Version option
    │               └─→ Replace in question
```

---

## 4. Concurrency Handling Flow

### Two-Layer Concurrency Strategy

```
┌─────────────────────────────────────────────────────────┐
│ Layer 1: Repository (QuestionnaireTemplateRepository)   │
└─────────────────────────────────────────────────────────┘
    │
    ├─→ SaveChangesAsync()
    │   │
    │   ├─→ [Attempt 1-3: Retry Loop]
    │   │   │
    │   │   ├─→ [If DbUpdateConcurrencyException]
    │   │   │   │
    │   │   │   ├─→ Refresh RowVersion from DB
    │   │   │   │   └─→ entry.OriginalValues.SetValues(databaseValues)
    │   │   │   │
    │   │   │   ├─→ Wait: 25ms * attempt
    │   │   │   │
    │   │   │   └─→ Retry SaveChangesAsync()
    │   │   │
    │   │   └─→ [If Unique Constraint Violation]
    │   │       └─→ Bubble up to Orchestrator
    │   │
    │   └─→ [Success] Return rows affected
    │
    └─→ [Failure after 3 attempts] Throw exception

┌─────────────────────────────────────────────────────────┐
│ Layer 2: Orchestrator (QuestionnaireTemplateOrchestrator)│
└─────────────────────────────────────────────────────────┘
    │
    ├─→ VersionTemplateAsync() / VersionTemplateForQuestionAsync()
    │   │
    │   ├─→ [Attempt 1-5: Retry Loop]
    │   │   │
    │   │   ├─→ [If Unique Constraint Violation]
    │   │   │   │
    │   │   │   ├─→ Calculate delay: BaseDelay * 2^(attempt-1) + jitter
    │   │   │   │
    │   │   │   ├─→ Refresh source template (NoTracking)
    │   │   │   │
    │   │   │   ├─→ Recalculate next version
    │   │   │   │
    │   │   │   └─→ Retry VersionTemplateInternalAsync()
    │   │   │
    │   │   └─→ [If Concurrency Exception]
    │   │       │
    │   │       ├─→ Calculate delay: BaseDelay * 2^(attempt-1) + jitter
    │   │       │
    │   │       ├─→ Refresh source template (NoTracking)
    │   │       │
    │   │       └─→ Retry VersionTemplateInternalAsync()
    │   │
    │   └─→ [Success] Return new template version
    │
    └─→ [Failure after 5 attempts] Throw exception
```

### Concurrency Scenarios

#### Scenario 1: Concurrent Template Versioning
```
Time    User A                          User B
─────────────────────────────────────────────────────────
T0      Fetch latest version (v5)        Fetch latest version (v5)
T1      Calculate next: v6              Calculate next: v6
T2      Create template v6              Create template v6
T3      SaveChangesAsync()              SaveChangesAsync()
T4      ✅ Success                      ❌ Unique Constraint Violation
T5                                      Retry: Fetch latest (v6)
T6                                      Calculate next: v7
T7                                      Create template v7
T8                                      ✅ Success
```

#### Scenario 2: RowVersion Conflict (Direct Update)
```
Time    User A                          User B
─────────────────────────────────────────────────────────
T0      Load template (RowVersion: 0x01) Load template (RowVersion: 0x01)
T1      Modify template                 Modify template
T2      SaveChangesAsync()              SaveChangesAsync()
T3      ✅ Success (RowVersion: 0x02)   ❌ DbUpdateConcurrencyException
T4                                      Refresh RowVersion: 0x02
T5                                      Retry SaveChangesAsync()
T6                                      ✅ Success (RowVersion: 0x03)
```

---

## 5. Version Calculation Flow

### Template Version Calculation
```
CalculateNextTemplateVersionAsync(templateName, currentVersion)
    │
    ├─→ Fetch latest version (NoTracking)
    │   └─→ QuestionnaireTemplateLatestByNameNoTrackingSpec
    │
    └─→ Return: (latest?.Version ?? currentVersion) + 1
```

### Question Version Calculation
```
VersionQuestionAsync(sourceQuestion, newSectionId)
    │
    ├─→ Fetch latest question version (NoTracking)
    │   └─→ QuestionLatestByNameSpec
    │
    ├─→ Calculate: (latest?.Version ?? source.Version) + 1
    │
    ├─→ Create new question instance
    │   └─→ Name = source.Name
    │   └─→ Version = nextVersion
    │   └─→ QuestionnaireSectionId = newSectionId
    │
    └─→ Return new question (not tracked)
```

### Option Version Calculation
```
VersionOptionAsync(sourceOption, newQuestionName, newQuestionVersion)
    │
    ├─→ Fetch latest option version (NoTracking)
    │   └─→ OptionLatestByNameSpec
    │
    ├─→ Calculate: (latest?.Version ?? source.Version) + 1
    │
    ├─→ Create new option instance
    │   └─→ Name = source.Name
    │   └─→ Version = nextVersion
    │   └─→ QuestionnaireQuestionName = newQuestionName
    │   └─→ QuestionnaireQuestionVersion = newQuestionVersion
    │
    └─→ Return new option (not tracked)
```

---

## 6. Entity Tracking Strategy

### NoTracking Usage
```
✅ ALWAYS Use NoTracking For:
    - Fetching latest versions (for calculation)
    - Loading source templates (for versioning)
    - Checking if version exists
    - Reading reference data

❌ NEVER Use NoTracking For:
    - Entities being modified directly
    - Entities being added to collections
    - Entities in tracked collections
```

### Entity Creation Strategy
```
✅ ALWAYS Create New Instances:
    - When versioning (never reuse tracked entities)
    - When adding to collections
    - When creating new entities

❌ NEVER Reuse Tracked Entities:
    - Don't modify and re-add tracked entities
    - Don't detach and re-attach entities
    - Always create fresh instances
```

---

## 7. Error Handling Flow

### Error Types and Responses

```
┌─────────────────────────────────────────────────────────┐
│ Error Type                    │ Handling               │
├─────────────────────────────────────────────────────────┤
│ Unique Constraint Violation   │ Retry with version     │
│ (Concurrent versioning)        │ recalculation          │
├─────────────────────────────────────────────────────────┤
│ DbUpdateConcurrencyException   │ Retry with RowVersion  │
│ (RowVersion conflict)          │ refresh                │
├─────────────────────────────────────────────────────────┤
│ Template Not Found            │ Throw exception         │
├─────────────────────────────────────────────────────────┤
│ Template In Use (no version)  │ Throw error with        │
│                                │ guidance message        │
├─────────────────────────────────────────────────────────┤
│ Validation Error              │ Throw with error list   │
└─────────────────────────────────────────────────────────┘
```

---

## 8. Complete Request Flow Example

### Example: User Versions Template with Questions

```
1. Frontend Request
   PUT /QuestionnaireTemplate/MyTemplate/3
   {
     "name": "MyTemplate",
     "version": 3,
     "shouldUpdateVersion": true,
     "sections": [...]
   }

2. QuestionnaireTemplateService.UpdateAsync()
   ├─→ Validate DTO ✅
   ├─→ Check: IsTemplateInUseAsync("MyTemplate", 3) → true
   ├─→ Check: shouldUpdateVersion → true
   └─→ Call: VersionTemplateAsync()

3. QuestionnaireTemplateOrchestrator.VersionTemplateAsync()
   ├─→ Load source template (NoTracking) ✅
   ├─→ Retry Loop (Max 5 attempts)
   │   │
   │   └─→ VersionTemplateInternalAsync()
   │       ├─→ CalculateNextTemplateVersionAsync()
   │       │   └─→ Fetch latest: v5
   │       │   └─→ Calculate: v6
   │       │
   │       ├─→ Check if v6 exists → No
   │       │
   │       ├─→ CreateNewTemplateEntity() → Template v6
   │       │
   │       ├─→ For each section:
   │       │   ├─→ VersionSectionAsync()
   │       │   │   ├─→ Create new Section (new Guid)
   │       │   │   │
   │       │   │   └─→ For each question:
   │       │   │       ├─→ VersionOrCreateQuestionAsync()
   │       │   │       │   ├─→ VersionQuestionAsync()
   │       │   │       │   │   └─→ Calculate: question v3
   │       │   │       │   │
   │       │   │       │   └─→ For each option:
   │       │   │       │       ├─→ VersionOptionAsync()
   │       │   │       │       │   └─→ Calculate: option v2
   │       │   │       │       │
   │       │   │       │       └─→ AddAsync(newOption)
   │       │   │       │
   │       │   │       └─→ AddAsync(newQuestion)
   │       │   │
   │       │   └─→ Add section to template
   │       │
   │       ├─→ AddAsync(newTemplate)
   │       │
   │       └─→ SaveChangesAsync()
   │           ├─→ Repository: Handle RowVersion conflicts ✅
   │           └─→ Success: All entities saved atomically
   │
   └─→ Return: New Template v6 DTO

4. Response to Frontend
   {
     "name": "MyTemplate",
     "version": 6,
     "status": "Draft",
     ...
   }
```

---

## 9. Key Principles

### ✅ DO:
- Always use NoTracking for read-only queries
- Always create new instances when versioning
- Always retry on unique constraint violations
- Always calculate versions from latest in database
- Always check if template is in use before allowing edits
- Always version all related entities (sections, questions, options)

### ❌ DON'T:
- Don't reuse tracked entities
- Don't modify entities during versioning
- Don't skip version checks
- Don't allow direct edits when template is in use
- Don't forget to version related entities
- Don't use tracking for reference data

---

## 10. Summary

The workflow ensures:
1. **Data Integrity**: All versioning is atomic and consistent
2. **Concurrency Safety**: Handles simultaneous operations gracefully
3. **User Experience**: Clear error messages guide users
4. **Performance**: Efficient queries with NoTracking
5. **Maintainability**: Clean separation of concerns
6. **Scalability**: Retry logic prevents database overload

The system is production-ready and handles all edge cases for concurrent template versioning operations.
