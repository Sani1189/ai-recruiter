import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";

import { JobPost } from "@/types/v2/type.view";
import { columns } from "./columns";

export default function JobPostsTable({ jobPosts }: { jobPosts: JobPost[] }) {
  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle>Your Job Openings</CardTitle>
      </CardHeader>

      <CardContent>
        <DataTable
          columns={columns}
          data={jobPosts}
          metadata={{
            searchField: "job title",
            placeholder: "Search by Job Title",
          }}
        />
      </CardContent>
    </Card>
  );
}
