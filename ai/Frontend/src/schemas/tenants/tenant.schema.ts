import { z } from "zod";

const TenantSchema = z.object({
  subDomain: z
    .string()
    .min(2, "SubDomain must be at least 2 characters")
    .max(20, "SubDomain max length is 20"),
  tenantName: z
    .string()
    .min(2, "TenantName must be at least 2 characters")
    .max(200, "TenantName max length is 200"),
  mobile: z.string().max(20, "Mobile max length is 20").or(z.literal("")),
  email: z.email("Invalid email address").max(200, "Email max length is 200"),
  web: z.string().max(200, "Web max length is 200").or(z.literal("")),
  address1: z
    .string()
    .min(1, "Address1 is required")
    .max(150, "Address1 max length is 150"),
  address2: z.string().max(100, "Address2 max length is 100").or(z.literal("")),
  city: z.string().max(100, "City max length is 100").or(z.literal("")),
  state: z.string().max(100, "State max length is 100").or(z.literal("")),
  country: z.string().max(2, "Country max length is 2").or(z.literal("")),
  zip: z.string().max(50, "Zip max length is 50").or(z.literal("")),
});
type TenantSchema = z.infer<typeof TenantSchema>;

export { TenantSchema };
