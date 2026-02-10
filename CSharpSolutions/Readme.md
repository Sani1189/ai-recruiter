# **Project Architecture & Design Plan**

## **1. Solution Structure Overview**
```
CSharpSolutions/
└── Recruiter/
    ├── Domain/                     # Domain-driven design layer
    │   ├── Models/
    │   │   └── InterviewEntity.cs
    │   └── Recruiter.Domain.csproj
    ├── Infrastructure/             # Shared infrastructure components
    │   ├── Repository/
    │   │   └── RecruiterDbContext.cs
    │   ├── Ardalis/               # Specification & repository helpers
    │   └── Migrations/            # EF Core migration scripts
    │   └── Recruiter.Infrastructure.csproj
    ├── Application/               # Application layer - use cases & services
    │   ├── Interview/
    │   │   ├── Specifications/
    │   │   │   ├── InterviewsByHighestScoreSpec.cs
    │   │   │   └── InterviewsByCandidateSpec.cs
    │   │   ├── Queries/
    │   │   │   ├── GetInterviewsByHighestScoreQuery.cs
    │   │   │   └── GetInterviewsByCandidateQuery.cs
    │   │   ├── Dto/
    │   │   │   └── InterviewDto.cs
    │   │   ├── InterviewProfile.cs    # AutoMapper profile
    │   │   ├── InterviewService.cs    # Uses only Interview repo/data
    │   │   ├── InterviewOrchestrator.cs # Coordinates across services
    │   │   └── InterviewInputValidator.cs
    │   └── Recruiter.Application.csproj
    ├── WebApi/                    # API Layer
    │   ├── Program.cs
    │   ├── Infrastructure/
    │   │   ├── Service.cs
    │   │   └── DataContextSetup.cs
    │   ├── Endpoints/
    │   │   ├── InterviewController.cs
    │   │   ├── JobsController.cs
    │   │   └── CandidateController.cs
    │   ├── Middleware/
    │   └── Recruiter.WebApi.csproj
    ├── UnitTests/                 # Unit Testing Project
    │   └── Recruiter.UnitTests.csproj
    └── CVProcessor/               # Microservice (Azure Durable Function)
        └── Recruiter.CVProcessor.csproj
```

## **2. Layer Dependencies & Relationships**

### **Project Reference Structure**

**WebApi Layer** (HTTP Endpoints & Controllers)
References: Application + Infrastructure

**Application Layer** (Use Cases & Services)  
References: Domain

**Domain Layer** (Business Rules)
References: NONE (Pure business logic)

**Infrastructure Layer** (Database & External APIs)
References: Domain + Application

**CVProcessor Layer** (Microservice)
References: Domain + Application

**UnitTests Layer** (Testing)
References: All Layers

### **Dependency Rules**

**Domain Layer**
Dependencies: NONE
Reason: Pure business logic
Example: Interview.CanBeRescheduled()

**Application Layer** 
Dependencies: Domain
Reason: Define contracts & use cases
Example: IInterviewRepository, InterviewService

**Infrastructure Layer**
Dependencies: Domain + Application
Reason: Implement contracts
Example: SqlInterviewRepository implements IInterviewRepository

**WebApi Layer**
Dependencies: Application + Infrastructure  
Reason: HTTP orchestration
Example: InterviewController uses InterviewService

**CVProcessor Layer**
Dependencies: Domain + Application
Reason: Microservice logic
Example: Uses ICandidateService for business operations

**UnitTests Layer**
Dependencies: All Layers
Reason: Testing purposes  
Example: Mock IInterviewRepository to test InterviewService

### **Key Architectural Benefits**

**Dependency Inversion**
High-level modules (Application) define interfaces
Low-level modules (Infrastructure) implement interfaces  
Business logic (Domain) remains pure and testable