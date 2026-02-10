import {
  Mail,
  Server,
  ShieldCheck,
  Key,
  Globe,
  Pencil,
  ToggleLeft,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { buttonVariants } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";

import { TEMP } from "@/constants/temp";
import { saasApiClient } from "@/lib/api/client";
import Link from "next/link";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "Email Setup | Recruiter",
  description: "Tenant email configuration",
};

interface TenantEmailSetting {
  emailSettingId: string;
  tenantId: string;
  providerType?: string | null;
  providerName?: string | null;
  displayName?: string | null;
  fromEmail?: string | null;
  fromName?: string | null;
  host?: string | null;
  port?: number | null;
  username?: string | null;
  passwordEncrypted?: string | null;
  apiKeyEncrypted?: string | null;
  enableSsl: boolean;
  useStartTls: boolean;
  isActive: boolean;
  isDefault: boolean;
  createdAt: string;
}

/* ---------- Helpers ---------- */
const hasValue = (v?: string | number | null) =>
  v !== null && v !== undefined && v !== "";

const hasAny = (...values: Array<string | number | boolean | null | undefined>) =>
  values.some(v => v !== null && v !== undefined && v !== "");

/* ---------- Empty model ---------- */
const EMPTY_SETTING: TenantEmailSetting = {
  emailSettingId: "",
  tenantId: "",
  providerType: null,
  providerName: null,
  displayName: null,
  fromEmail: null,
  fromName: null,
  host: null,
  port: null,
  username: null,
  passwordEncrypted: null,
  apiKeyEncrypted: null,
  enableSsl: false,
  useStartTls: false,
  isActive: false,
  isDefault: false,
  createdAt: "",
};

export default async function EmailSetupPage() {
  let setting: TenantEmailSetting = EMPTY_SETTING;
  let hasRealData = false;

  try {
    const response = await saasApiClient.get<TenantEmailSetting>(
      `/tenantemailsettings/${TEMP.tenantId}`,
      { cacheStrategy: "no-cache" },
    );

    if (response.success && response.data) {
      setting = response.data;
      hasRealData = true;
    }
  } catch {
    // ❗ Intentionally silent
    // We render the page with EMPTY_SETTING
  }

  return (
    <div className="from-primary/5 via-background to-secondary/5 min-h-screen bg-gradient-to-br py-12">
      <div className="container px-20">
        {/* Page Header */}
        <div className="mb-8 space-y-2">
          <h1 className="text-3xl font-bold">Email Configuration</h1>
          <p className="text-muted-foreground">
            Tenant email provider and sending configuration
          </p>
        </div>

        <div className="mx-auto max-w-4xl">
          <Card>
            {/* Header */}
            <CardHeader className="flex flex-row items-start justify-between space-y-0">
              <div>
                <CardTitle className="flex items-center gap-2">
                  <Mail className="h-5 w-5" />
                  Email Settings
                </CardTitle>
                <CardDescription>
                  {hasRealData
                    ? "Read-only email configuration"
                    : "No email configuration found"}
                </CardDescription>
              </div>

              <Link
                href="/recruiter/tenants/email-setup/edit"
                className={buttonVariants({ variant: "outline", size: "icon" })}
              >
                <Pencil className="h-4 w-4" />
              </Link>
            </CardHeader>

            <CardContent className="space-y-8">
              {/* Provider Header */}
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-2xl font-semibold">
                    {setting.providerName || "Email Provider"}
                  </h2>

                  {hasValue(setting.providerType) && (
                    <p className="text-muted-foreground">
                      Type:{" "}
                      <span className="font-medium">
                        {setting.providerType}
                      </span>
                    </p>
                  )}
                </div>

                {hasRealData && (
                  <div className="flex gap-2">
                    {setting.isDefault && (
                      <Badge variant="secondary">Default</Badge>
                    )}
                    <Badge
                      variant={setting.isActive ? "default" : "secondary"}
                      className="gap-1"
                    >
                      <ShieldCheck className="h-3 w-3" />
                      {setting.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </div>
                )}
              </div>

              {/* Sender Information */}
              {hasAny(
                setting.fromEmail,
                setting.fromName,
                setting.displayName
              ) && (
                <>
                  <Separator />
                  <Section title="Sender Information">
                    {hasValue(setting.fromEmail) && (
                      <Info
                        icon={<Mail />}
                        label="From Email"
                        value={setting.fromEmail!}
                      />
                    )}
                    {hasValue(setting.fromName) && (
                      <Info
                        icon={<Globe />}
                        label="From Name"
                        value={setting.fromName!}
                      />
                    )}
                    {hasValue(setting.displayName) && (
                      <Info
                        icon={<Globe />}
                        label="Display Name"
                        value={setting.displayName!}
                      />
                    )}
                  </Section>
                </>
              )}

              {/* Server Configuration */}
              {hasAny(setting.host, setting.port, setting.username) && (
                <>
                  <Separator />
                  <Section title="Server Configuration">
                    {hasValue(setting.host) && (
                      <Info
                        icon={<Server />}
                        label="Host"
                        value={setting.host!}
                      />
                    )}
                    {hasValue(setting.port) && (
                      <Info
                        icon={<Server />}
                        label="Port"
                        value={setting.port!.toString()}
                      />
                    )}
                    {hasValue(setting.username) && (
                      <Info
                        icon={<Mail />}
                        label="Username"
                        value={setting.username!}
                      />
                    )}
                    <Info
                      icon={<Key />}
                      label="Authentication"
                      value={
                        setting.apiKeyEncrypted
                          ? "API Key"
                          : setting.passwordEncrypted
                          ? "Password"
                          : "None"
                      }
                    />
                  </Section>
                </>
              )}

              {/* Security */}
              {(setting.enableSsl || setting.useStartTls) && (
                <>
                  <Separator />
                  <Section title="Security & Flags">
                    <BooleanInfo
                      label="Enable SSL"
                      value={setting.enableSsl}
                    />
                    <BooleanInfo
                      label="Use STARTTLS"
                      value={setting.useStartTls}
                    />
                  </Section>
                </>
              )}

              {/* Empty content hint */}
              {!hasRealData && (
                <p className="text-muted-foreground text-sm">
                  Configure email settings to enable system emails.
                </p>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

/* ---------- UI Helpers ---------- */

function Section({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-4">
      <h3 className="text-lg font-semibold">{title}</h3>
      <div className="grid gap-6 md:grid-cols-2">{children}</div>
    </div>
  );
}

function Info({
  icon,
  label,
  value,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
}) {
  return (
    <div className="flex items-center gap-3">
      <span className="text-muted-foreground">{icon}</span>
      <div>
        <div className="text-muted-foreground text-sm">{label}</div>
        <div className="font-medium">{value}</div>
      </div>
    </div>
  );
}

function BooleanInfo({
  label,
  value,
}: {
  label: string;
  value: boolean;
}) {
  return (
    <div className="flex items-center justify-between rounded-md border p-3">
      <span className="text-sm font-medium">{label}</span>
      <Badge variant={value ? "default" : "secondary"}>
        <ToggleLeft className="mr-1 h-3 w-3" />
        {value ? "Yes" : "No"}
      </Badge>
    </div>
  );
}
