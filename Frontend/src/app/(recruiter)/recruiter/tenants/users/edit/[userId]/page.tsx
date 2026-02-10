import { notFound } from "next/navigation";

import UserForm from "@/components/pages/_recruiter/recruiter/tenants/users/userForm";

import { User } from "@/components/pages/_recruiter/recruiter/tenants/users/column";
import { TEMP } from "@/constants/temp";
import { saasApiClient } from "@/lib/api/client";

export const dynamic = "force-dynamic";

type PageProps = {
  params: Promise<{ userId: string }>;
};
export default async function Page({ params }: PageProps) {
  const { userId } = await params;
  let user;

  try {
    const response = await saasApiClient.get<User>(
      `/TenantUsers/${TEMP.tenantId}/${userId}`,
    );

    if (!response.success) {
      notFound();
    }

    user = response.data;
  } catch (error) {
    notFound();
  }

  return <UserForm mode="edit" user={user} userId={userId} />;
}
