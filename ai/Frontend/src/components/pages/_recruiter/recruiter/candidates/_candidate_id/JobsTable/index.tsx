import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DataTable } from "@/components/ui/data-table-v2/data-table";

import { columns } from "./columns";

export default function JobsTable({
  attendedInterviews,
}: {
  attendedInterviews: any[];
}) {
  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="flex items-center">
          All Applied Jobs
        </CardTitle>
      </CardHeader>

      <CardContent>
        <DataTable
          columns={columns}
          data={attendedInterviews}
          metadata={{
            searchField: "title",
            placeholder: "Search by Job Title",
            defaultHidden: ["averageScore"],
          }}
        />
      </CardContent>
    </Card>
  );
}
