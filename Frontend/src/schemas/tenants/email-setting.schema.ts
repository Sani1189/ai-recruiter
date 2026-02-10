import { z } from "zod";

/**
 * Email provider types
 */
export const EmailProviderEnum = z.enum(["SMT", "API"]);
export type EmailProviderType = z.infer<typeof EmailProviderEnum>;

/**
 * Core Email Setting Schema
 */
export const TenantEmailSettingSchema = z.object({
  id: z.string().uuid().optional(),

  tenantId: z.string().uuid(),

  providerType: EmailProviderEnum,
  providerName: z.string().optional().nullable(),

  /* ---------- Sender Info ---------- */
  fromEmail: z.string().email("Invalid email").optional().nullable(),
  fromName: z.string().optional().nullable(),
  displayName: z.string().optional().nullable(),

  /* ---------- SMTP Settings ---------- */
  host: z.string().optional().nullable(),
  port: z.number().int().positive().optional().nullable(),
  username: z.string().optional().nullable(),
  passwordEncrypted: z.string().optional().nullable(),

  enableSsl: z.boolean(),      // Required boolean
  useStartTls: z.boolean(),    // Required boolean

  /* ---------- API Provider Settings ---------- */
  apiKeyEncrypted: z.string().optional().nullable(),

  /* ---------- Meta ---------- */
  isActive: z.boolean(),       // Required boolean
  isDefault: z.boolean(),      // Required boolean

  createdAt: z.string().optional(),
  updatedAt: z.string().optional(),
});

/**
 * Type inference
 */
export type TenantEmailSettingSchemaType = z.infer<
  typeof TenantEmailSettingSchema
>;

/**
 * Safe default values for CREATE mode
 */
export const EmailSettingDefaultValues: TenantEmailSettingSchemaType = {
  tenantId: "",
  providerType: "SMT",
  providerName: "",

  fromEmail: "",
  fromName: "",
  displayName: "",

  host: "",
  port: 587,
  username: "",
  passwordEncrypted: "",

  enableSsl: false,
  useStartTls: true,

  apiKeyEncrypted: "",

  isActive: true,
  isDefault: false,
};
