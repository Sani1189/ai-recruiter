import { Plus } from "lucide-react";
import { Metadata } from "next";
import Link from "next/link";

import UsersTable from "@/components/pages/_recruiter/recruiter/tenants/users/table";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = {
  title: "Users | Recruiter",
  description: "Manage users for your recruitment process",
};

export default function Page() {
  return (
    <div className="container space-y-6 py-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Users</h1>
          <p className="text-muted-foreground">
            Manage users for your recruitment process
          </p>
        </div>

        <Link href="/recruiter/tenants/users/invite">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Send Invitaion
          </Button>
        </Link>
        {/* <Link href="/recruiter/tenants/users/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            New User
          </Button>
        </Link> */}
      </div>

      <UsersTable />
    </div>
  );
}
