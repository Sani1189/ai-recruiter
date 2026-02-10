import z from "zod";

const ServiceSchema = z.object({
  serviceName: z
    .string()
    .min(1, "Service name is required")
    .max(100, "Service name must be less than 100 characters"),
});
type ServiceSchema = z.infer<typeof ServiceSchema>;

export { ServiceSchema };
