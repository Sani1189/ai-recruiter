"use client";

import { Building2, Check, Gift, Sparkles, Zap } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { Badge } from "@/components/ui/badge";
import { TEMP } from "@/constants/temp";
import { useApi } from "@/hooks/useApi";
import { Service, ServiceTier } from "@/types/service";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

const TIER_CONFIG: Record<
  "free" | "basic" | "pro" | "enterprise",
  {
    name: string;
    icon: typeof Sparkles;
    color: string;
    borderColor: string;
    badgeColor: string;
    buttonVariant: "outline" | "default";
    popular?: boolean;
  }
> = {
  free: {
    name: "Free",
    icon: Gift,
    color: "from-green-500/20 to-green-600/20",
    borderColor: "border-green-500/30 hover:border-green-500/60",
    badgeColor: "bg-green-500/10 text-green-500",
    buttonVariant: "outline",
  },
  basic: {
    name: "Basic",
    icon: Sparkles,
    color: "from-blue-500/20 to-blue-600/20",
    borderColor: "border-blue-500/30 hover:border-blue-500/60",
    badgeColor: "bg-blue-500/10 text-blue-500",
    buttonVariant: "outline",
  },
  pro: {
    name: "Pro",
    icon: Zap,
    color: "from-brand/20 to-brand-secondary/20",
    borderColor: "border-brand/30 hover:border-brand/60",
    badgeColor: "bg-brand/10 text-brand",
    buttonVariant: "default",
    popular: true,
  },
  enterprise: {
    name: "Enterprise",
    icon: Building2,
    color: "from-purple-500/20 to-purple-600/20",
    borderColor: "border-purple-500/30 hover:border-purple-500/60",
    badgeColor: "bg-purple-500/10 text-purple-500",
    buttonVariant: "outline",
  },
};

const TIER_ORDER = Object.keys(TIER_CONFIG) as (keyof typeof TIER_CONFIG)[];

export default function ServiceForm({ services }: { services: Service[] }) {
  const [loading, setLoading] = useState(false);

  const api = useApi(true);
  const router = useRouter();

  const [selectedServiceId, setSelectedServiceId] = useState<string>("");
  const selectedService = services.find(
    (s) => s.serviceId === selectedServiceId,
  );

  const handleSelectPlan = async (tier: ServiceTier) => {
    if (!selectedService || !tier.packageId) return;

    try {
      setLoading(true);

      const payload = {
        tenantId: TEMP.tenantId,
        serviceId: selectedService.serviceId,
        serviceSourceId: tier.packageId,
      };

      const response = await api.post("/tenantservices", payload, {
        cacheStrategy: "no-cache",
      });
      if (!response.success) {
        throw new Error(response.message || "Failed to add service");
      }

      toast.success("Service added successfully");
      router.push("/recruiter/tenants/services");
    } catch (error) {
      toast.error((error as Error).message || "An unexpected error occurred");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-background min-h-screen">
      <div className="container mx-auto px-4 py-12">
        {/* Header */}
        <div className="mb-12 text-center">
          <h1 className="mb-4 text-4xl font-bold">Add New Service</h1>
          <p className="text-muted-foreground mx-auto max-w-2xl text-lg">
            Select a service to view available plans and pricing tiers
          </p>
        </div>

        {/* Service Selection */}
        <div className="mx-auto mb-12 max-w-md">
          <label className="mb-2 block text-sm font-medium">
            Select a Service
          </label>
          <Select
            value={selectedServiceId}
            onValueChange={setSelectedServiceId}
          >
            <SelectTrigger className="h-12 w-full text-base">
              <SelectValue placeholder="Choose a service..." />
            </SelectTrigger>
            <SelectContent>
              {services.map((service) => (
                <SelectItem key={service.serviceId} value={service.serviceId}>
                  {service.serviceName}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        {/* Service Details */}
        {selectedService && (
          <div className="animate-in fade-in slide-in-from-bottom-4 duration-500">
            {/* Service Info */}
            <div className="mb-10 text-center">
              <h2 className="mb-2 text-2xl font-semibold">
                {selectedService.serviceName}
              </h2>
            </div>

            {/* Pricing Tiers */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 xl:grid-cols-4">
              {TIER_ORDER.map((tier) => {
                const config = TIER_CONFIG[tier];
                const Icon = config.icon;
                const tierData: ServiceTier = selectedService[tier];

                return (
                  <Card
                    key={tier}
                    className={`relative overflow-hidden transition-all duration-300 ${config.borderColor} hover:shadow-lg ${config.popular ? "ring-brand ring-2" : ""}`}
                  >
                    {/* Glossy background */}
                    <div
                      className={`absolute inset-0 bg-gradient-to-br ${config.color} opacity-50`}
                    />
                    <div className="absolute inset-0 backdrop-blur-xl" />

                    {/* Popular badge */}
                    {config.popular && (
                      <div className="absolute top-4 right-4">
                        <Badge className="bg-brand text-white">
                          Most Popular
                        </Badge>
                      </div>
                    )}

                    <CardHeader className="relative">
                      <div
                        className={`h-12 w-12 rounded-xl bg-gradient-to-br ${config.color} mb-4 flex items-center justify-center`}
                      >
                        <Icon className="h-6 w-6" />
                      </div>
                      <CardTitle className="text-xl">{config.name}</CardTitle>
                      <CardDescription className="pt-2">
                        <span className="text-foreground text-2xl font-bold">
                          {tierData.price === "-" || tierData.price === "0.00"
                            ? tierData.price === "0.00"
                              ? "Free"
                              : "Contact Us"
                            : `$${tierData.price}`}
                        </span>
                        {tierData.price !== "-" &&
                          tierData.price !== "0.00" && (
                            <span className="text-muted-foreground text-sm">
                              /month
                            </span>
                          )}
                      </CardDescription>
                    </CardHeader>

                    <CardContent className="relative flex grow flex-col justify-between space-y-6">
                      {/* Tier Details */}
                      <div className="min-h-[100px] space-y-2">
                        {tierData.features.length > 0 ? (
                          tierData.features.map((feature, idx) => (
                            <div key={idx} className="flex items-start gap-2">
                              <Check className="text-primary mt-0.5 h-5 w-5 shrink-0" />
                              <div className="text-sm">
                                <span>{feature.featureName}</span>
                                <Badge
                                  variant="secondary"
                                  className="ml-2 text-xs"
                                >
                                  {feature.featureValue}
                                </Badge>
                              </div>
                            </div>
                          ))
                        ) : (
                          <p className="text-muted-foreground text-sm italic">
                            Contact sales for custom features
                          </p>
                        )}
                      </div>

                      {/* Action Button */}
                      <Button
                        variant={config.buttonVariant}
                        className="w-full"
                        size="lg"
                        isLoading={loading}
                        onClick={() => handleSelectPlan(tierData)}
                      >
                        Select {config.name}
                      </Button>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          </div>
        )}

        {/* Empty State */}
        {!selectedService && (
          <div className="py-16 text-center">
            <div className="bg-muted/50 mx-auto mb-6 flex h-24 w-24 items-center justify-center rounded-full">
              <Sparkles className="text-muted-foreground h-10 w-10" />
            </div>
            <p className="text-muted-foreground">
              Select a service from the dropdown above to view available plans
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
