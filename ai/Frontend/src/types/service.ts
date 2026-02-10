export interface ServiceFeature {
  featureName: string;
  unitCode: string;
  unitName: string;
  featureValue: number
}

export interface ServiceTier {
  price: string;
  packageId: string | null;
  features: ServiceFeature[];
}

export interface Service {
  serviceId: string;
  serviceName: string;
  free: ServiceTier;
  basic: ServiceTier;
  pro: ServiceTier;
  enterprise: ServiceTier;
}

export interface TenantServices {
  createdAt: string;
  description: string;
  serviceEndDate: string;
  serviceId: string;
  serviceName: string;
  servicePlan: string;
  serviceStartDate: string;
  serviceType: string;
  statusCode: "ACT" | "INA";
  statusDesc: "Active" | "Inactive";
  tenantId: string;
  tenantServiceId: string;
}
