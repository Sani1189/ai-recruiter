import EmailSetupForm from "@/components/pages/_tenant/email-setup/EmailSetupForm";
import { TEMP } from "@/constants/temp";
import { saasApiClient } from "@/lib/api/client";
import { TenantEmailSettingSchemaType, EmailSettingDefaultValues } from "@/schemas/tenants/email-setting.schema";

export const dynamic = "force-dynamic";

export default async function EmailSetupEditPage() {
  const tenantId = TEMP.tenantId;

  // Fetch existing email settings for this tenant
  const response = await saasApiClient.get<TenantEmailSettingSchemaType | null>(
    `/tenantemailsettings/${tenantId}`,
    { cacheStrategy: "no-cache" }
  );

  const emailSetting = response.success ? response.data : null;

  // If email setting exists, use edit mode
  if (emailSetting) {
    const defaultValues = {
      ...EmailSettingDefaultValues, 
      tenantId,
      providerType: emailSetting.providerType ?? "SMT",
      providerName: emailSetting.providerName ?? "",
      displayName: emailSetting.displayName ?? "",
      fromEmail: emailSetting.fromEmail ?? "",
      fromName: emailSetting.fromName ?? "",
      host: emailSetting.host ?? "",
      port: emailSetting.port ?? 587,
      username: emailSetting.username ?? "",
      passwordEncrypted: emailSetting.passwordEncrypted ?? "",
      apiKeyEncrypted: emailSetting.apiKeyEncrypted ?? "",
      enableSsl: emailSetting.enableSsl ?? false,
      useStartTls: emailSetting.useStartTls ?? true,
      isActive: emailSetting.isActive ?? true,
      isDefault: emailSetting.isDefault ?? false,
    };

    return (
      <div className="min-h-screen py-12 bg-muted/30">
        <div className="container mx-auto px-4">
          <EmailSetupForm
            mode="edit"
            tenantId={tenantId}
            emailSettingId={emailSetting.id || ""}
            defaultValues={defaultValues}
          />
        </div>
      </div>
    );
  }

  // If no email setting exists, fallback to create mode
  return (
    <div className="min-h-screen py-12 bg-muted/30">
      <div className="container mx-auto px-4">
        <EmailSetupForm mode="create" tenantId={tenantId} />
      </div>
    </div>
  );
}
