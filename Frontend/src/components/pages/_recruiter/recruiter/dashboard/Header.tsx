import { Plus } from "lucide-react";
import Link from "next/link";

import { buttonVariants } from "@/components/ui/button";

export default function Header() {
  return (
    <div className="xs:flex-row xs:items-center flex flex-col items-start justify-between gap-2">
      <div>
        <h1 className="mb-2 text-3xl font-bold">Recruiter Dashboard</h1>
        <p className="text-muted-foreground">
          Manage your Job Openings and review Candidate Performances
        </p>
      </div>
      <Link href="/recruiter/tenants/profiles/new" className={buttonVariants()}>
        <Plus className="mr-2 h-4 w-4" />
        Create Organization
      </Link>
      <Link href="/recruiter/jobs/new" className={buttonVariants()}>
        <Plus className="mr-2 h-4 w-4" />
        New Job Opening
      </Link>
    </div>
  );
}
