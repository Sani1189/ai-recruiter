import { Plus } from "lucide-react";
import { Metadata } from "next";
import Link from "next/link";

import ServicesTable from "@/components/pages/_recruiter/recruiter/tenants/services/table";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = {
  title: "Services | Recruiter",
  description: "Manage services for your recruitment process",
};

export default function Page() {
  return (
    <div className="container space-y-6 py-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Services</h1>
          <p className="text-muted-foreground">
            Manage services for your recruitment process
          </p>
        </div>

        <Link href="/recruiter/tenants/services/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Add New Service
          </Button>
        </Link>
      </div>

      <ServicesTable />
    </div>
  );
}
