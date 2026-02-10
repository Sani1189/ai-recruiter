"use client";

import EmailSetupForm from "@/components/pages/_tenant/email-setup/EmailSetupForm";
import { TEMP } from "@/constants/temp";

export default function EmailSetupCreatePage() {
  const tenantId = TEMP.tenantId;

  return (
    <div className="min-h-screen py-12 bg-muted/30">
      <div className="container mx-auto px-4">
        <EmailSetupForm mode="create" tenantId={tenantId} />
      </div>
    </div>
  );
}
