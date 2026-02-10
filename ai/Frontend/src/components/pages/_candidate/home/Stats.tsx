const STATS = [
  {
    value: "10,000+",
    label: "Active Candidates",
  },
  {
    value: "500+",
    label: "Partner Companies",
  },
  {
    value: "92%",
    label: "Success Rate",
  },
  {
    value: "50K+",
    label: "Interviews Completed",
  },
];

export default function Stats() {
  return (
    <section className="bg-muted py-16">
      <div className="xs:grid-cols-2 container grid gap-8 text-center md:grid-cols-4">
        {STATS.map((stat, index) => (
          <div key={index} className="space-y-2">
            <div className="text-primary text-3xl font-bold">{stat.value}</div>
            <div className="text-muted-foreground">{stat.label}</div>
          </div>
        ))}
      </div>
    </section>
  );
}
