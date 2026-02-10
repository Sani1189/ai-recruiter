import CandidatesTable from "@/components/pages/_recruiter/recruiter/candidates/CandidatesTable";

export default function CandidatesPage() {
  return (
    <div className="container space-y-8 py-8">
      <div>
        <h1 className="mb-2 text-3xl font-bold">Candidates</h1>
        <p className="text-muted-foreground">
          View and manage all candidates who have applied for your job openings
        </p>
      </div>

      <CandidatesTable />
    </div>
  );
}
