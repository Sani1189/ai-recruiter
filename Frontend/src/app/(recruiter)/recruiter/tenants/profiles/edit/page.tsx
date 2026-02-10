import { notFound } from "next/navigation";

import TenantProfileForm from "@/components/pages/_tenant/profile/TenantProfileForm";

import { TEMP } from "@/constants/temp";
import { saasApiClient } from "@/lib/api/client";
import { TenantSchema } from "@/schemas/tenants/tenant.schema";

export const dynamic = "force-dynamic";

export default async function TenantProfilePage() {
  const tenantId = TEMP.tenantId;
  const response = await saasApiClient.get<TenantSchema>(
    `/tenants/${tenantId}`,
    {
      cacheStrategy: "no-cache",
    },
  );

  if (!response.success) {
    notFound();
  }

  return (
    <TenantProfileForm mode="edit" tenantId={tenantId} tenant={response.data} />
  );
}
