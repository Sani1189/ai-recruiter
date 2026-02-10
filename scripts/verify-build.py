#!/usr/bin/env python3
import os
import json
import sys
from pathlib import Path

def verify_files_exist():
    """Verify all critical files have been created and updated"""
    files_to_check = [
        # C# Models
        "ai/CSharpSolutions/Recruiter/Domain/Models/KanbanBoardColumn.cs",
        "ai/CSharpSolutions/Recruiter/Domain/Models/JobPost.cs",
        
        # C# DTOs
        "ai/CSharpSolutions/Recruiter/Application/JobPost/Dto/JobPostDto.cs",
        "ai/CSharpSolutions/Recruiter/Application/JobPost/Dto/KanbanBoardColumnDto.cs",
        
        # C# Services
        "ai/CSharpSolutions/Recruiter/Application/JobPost/KanbanBoardColumnService.cs",
        "ai/CSharpSolutions/Recruiter/Application/JobPost/Specifications/KanbanBoardColumnByRecruiterSpec.cs",
        
        # C# Controllers
        "ai/CSharpSolutions/Recruiter/WebApi/Endpoints/KanbanBoardColumnController.cs",
        "ai/CSharpSolutions/Recruiter/WebApi/Endpoints/JobController.cs",
        
        # Migrations
        "ai/CSharpSolutions/Recruiter/Infrastructure/Migrations/20260210000000_AddKanbanBoardColumnsAndJobPostFields.cs",
        "ai/CSharpSolutions/Recruiter/Infrastructure/Migrations/20260210000000_AddKanbanBoardColumnsAndJobPostFields.Designer.cs",
        
        # Database Context
        "ai/CSharpSolutions/Recruiter/Infrastructure/Repository/RecruiterDbContext.cs",
        
        # Frontend Services
        "ai/Frontend/src/lib/api/services/jobs.service.ts",
        
        # Frontend Components
        "ai/Frontend/src/components/JobPostForm/JobPostDetailsStep.tsx",
        "ai/Frontend/src/components/pages/_recruiter/recruiter/jobs/KanbanBoardColumnsManager.tsx",
        
        # Documentation
        "IMPLEMENTATION_SUMMARY.md",
        "INTEGRATION_GUIDE.md",
        "CHANGES.md",
        "README_UPDATES.md",
        "ARCHITECTURE_DIAGRAM.md",
    ]
    
    missing_files = []
    existing_files = []
    
    for file_path in files_to_check:
        full_path = Path(file_path)
        if full_path.exists():
            existing_files.append(file_path)
            print(f"✓ {file_path}")
        else:
            missing_files.append(file_path)
            print(f"✗ MISSING: {file_path}")
    
    print(f"\n{'='*60}")
    print(f"Files verified: {len(existing_files)}/{len(files_to_check)}")
    if missing_files:
        print(f"\nMissing files ({len(missing_files)}):")
        for f in missing_files:
            print(f"  - {f}")
        return False
    else:
        print("All required files exist!")
        return True

def check_key_implementations():
    """Check that key code implementations are in place"""
    checks = {
        "KanbanBoardColumn model has BaseDbModel": {
            "file": "ai/CSharpSolutions/Recruiter/Domain/Models/KanbanBoardColumn.cs",
            "pattern": "public class KanbanBoardColumn : BaseDbModel"
        },
        "JobPost has new fields": {
            "file": "ai/CSharpSolutions/Recruiter/Domain/Models/JobPost.cs",
            "pattern": "public string Industry"
        },
        "KanbanBoardColumnService implemented": {
            "file": "ai/CSharpSolutions/Recruiter/Application/JobPost/KanbanBoardColumnService.cs",
            "pattern": "public class KanbanBoardColumnService : IKanbanBoardColumnService"
        },
        "DbContext has KanbanBoardColumns": {
            "file": "ai/CSharpSolutions/Recruiter/Infrastructure/Repository/RecruiterDbContext.cs",
            "pattern": "public DbSet<KanbanBoardColumn> KanbanBoardColumns"
        },
        "Migration created": {
            "file": "ai/CSharpSolutions/Recruiter/Infrastructure/Migrations/20260210000000_AddKanbanBoardColumnsAndJobPostFields.cs",
            "pattern": "CreateTable(\"KanbanBoardColumns\")"
        },
    }
    
    print(f"\n{'='*60}")
    print("Key Implementation Checks:")
    print("="*60)
    
    all_passed = True
    for check_name, details in checks.items():
        file_path = Path(details["file"])
        if not file_path.exists():
            print(f"✗ {check_name} - FILE NOT FOUND")
            all_passed = False
            continue
        
        try:
            content = file_path.read_text()
            if details["pattern"] in content:
                print(f"✓ {check_name}")
            else:
                print(f"✗ {check_name} - PATTERN NOT FOUND")
                print(f"  Looking for: {details['pattern']}")
                all_passed = False
        except Exception as e:
            print(f"✗ {check_name} - ERROR: {e}")
            all_passed = False
    
    return all_passed

def main():
    print("="*60)
    print("BUILD VERIFICATION REPORT")
    print("="*60)
    
    files_ok = verify_files_exist()
    impl_ok = check_key_implementations()
    
    print(f"\n{'='*60}")
    print("SUMMARY")
    print("="*60)
    print(f"Files check:          {'PASSED' if files_ok else 'FAILED'}")
    print(f"Implementation check: {'PASSED' if impl_ok else 'FAILED'}")
    
    if files_ok and impl_ok:
        print("\n✓ All verifications passed! Ready to build.")
        print("\nNext steps:")
        print("1. Run: dotnet build")
        print("2. Run: dotnet run --project Recruiter/WebApi")
        print("3. Apply migrations: dotnet ef database update")
        sys.exit(0)
    else:
        print("\n✗ Some verifications failed. Please review above.")
        sys.exit(1)

if __name__ == "__main__":
    main()
