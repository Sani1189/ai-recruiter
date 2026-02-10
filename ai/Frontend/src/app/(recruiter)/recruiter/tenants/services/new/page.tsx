import { notFound } from "next/navigation";

import ServiceForm from "@/components/pages/_recruiter/recruiter/tenants/services/serviceForm";

import { saasApiClient } from "@/lib/api/client";
import { Service } from "@/types/service";

// Requires live API; do not prerender at build time.
export const dynamic = "force-dynamic";

export default async function Page() {
  const response = await saasApiClient.get<Service[]>(
    "/services/feature-comparison",
  );
  if (!response.success) {
    notFound();
  }

  const services = response.data;

  return <ServiceForm services={services} />;
}
