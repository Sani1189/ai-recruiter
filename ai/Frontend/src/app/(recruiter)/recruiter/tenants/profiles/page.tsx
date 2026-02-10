import {
  Building2,
  Globe,
  Mail,
  MapPin,
  Pencil,
  Phone,
  ShieldCheck,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";

/** Skip static prerender; page depends on runtime API (tenant profile). */
export const dynamic = "force-dynamic";
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
import { notFound } from "next/navigation";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "Tenant Profile | Recruiter",
  description: "Manage tenant profile information",
};


interface Tenant {
  tenantId: string;
  subDomain: string;
  tenantName: string;
  phone: string;
  mobile: string;
  email: string;
  web: string;
  address1: string;
  address2: string;
  city: string;
  state: string;
  country: string;
  zip: string;
  statusCode: string;
  statusDesc: string;
}

export default async function TenantProfilePage() {
  const response = await saasApiClient.get<Tenant>(
    `/tenants/${TEMP.tenantId}`,
    { cacheStrategy: "no-cache" },
  );
  if (!response.success) {
    notFound();
  }

  const tenant = response.data;

  return (
    <div className="from-primary/5 via-background to-secondary/5 min-h-screen bg-gradient-to-br py-12">
      <div className="container px-20">
        {/* Page Header */}
        <div className="mb-8 space-y-2">
          <h1 className="text-3xl font-bold">Tenant Profile</h1>
          <p className="text-muted-foreground">
            Tenant information and configuration
          </p>
        </div>

        <div className="mx-auto max-w-4xl">
          <Card>
            {/* Header with Edit button */}
            <CardHeader className="flex flex-row items-start justify-between space-y-0">
              <div>
                <CardTitle className="flex items-center gap-2">
                  <Building2 className="h-5 w-5" />
                  Tenant Information
                </CardTitle>
                <CardDescription>Basic tenant details</CardDescription>
              </div>

              <Link
                href="/recruiter/tenants/profiles/edit"
                className={buttonVariants({ variant: "outline", size: "icon" })}
              >
                <Pencil className="h-4 w-4" />
              </Link>
            </CardHeader>

            <CardContent className="space-y-8">
              {/* Tenant header */}
              <div className="flex items-center justify-between">
                <div>
                  <h2 className="text-2xl font-semibold">
                    {tenant.tenantName}
                  </h2>
                  <p className="text-muted-foreground">
                    Subdomain:{" "}
                    <span className="font-medium">{tenant.subDomain}</span>
                  </p>
                </div>

                <Badge
                  variant={
                    tenant.statusCode === "ACT" ? "default" : "secondary"
                  }
                  className="gap-1"
                >
                  <ShieldCheck className="h-3 w-3" />
                  {tenant.statusDesc}
                </Badge>
              </div>

              <Separator />

              {/* Contact */}
              <div className="grid gap-6 md:grid-cols-2">
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold">Contact Information</h3>

                  <Info icon={<Mail />} label="Email" value={tenant.email} />
                  <Info icon={<Phone />} label="Phone" value={tenant.phone} />
                  <Info icon={<Phone />} label="Mobile" value={tenant.mobile} />
                  <Info icon={<Globe />} label="Website" value={tenant.web} />
                </div>
              </div>

              <Separator />

              {/* Address */}
              <div className="space-y-4">
                <h3 className="text-lg font-semibold">Address</h3>

                <div className="flex gap-3">
                  <MapPin className="text-muted-foreground mt-1 h-4 w-4" />
                  <div>
                    <div className="font-medium">{tenant.address1}</div>
                    <div className="text-muted-foreground">
                      {tenant.address2}
                    </div>
                    <div className="text-muted-foreground">
                      {tenant.city}, {tenant.state} {tenant.zip}
                    </div>
                    <div className="text-muted-foreground">
                      {tenant.country}
                    </div>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

/* Small reusable row */
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
