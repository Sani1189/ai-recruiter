import z from "zod";

/* ---------------- Base User Schema ---------------- */
const UserSchema = z.object({
  firstName: z
    .string()
    .min(1, "First name is required")
    .max(50, "First name must be less than 100 characters"),
  lastName: z
    .string()
    .min(1, "Last name is required")
    .max(50, "Last name must be less than 100 characters"),
  email: z
    .email("Invalid email address")
    .max(250, "Email must be less than 250 characters"),
  userPass: z
    .string()
    .min(8, "Password must be at least 8 characters")
    .max(100, "Password must be less than 100 characters"),
  roleId: z.int("Invalid Role ID").positive("Invalid Role ID"),
  statusCode: z
    .string()
    .min(1, "Status is required")
    .max(50, "Status must be less than 50 characters"),
});
type UserSchema = z.infer<typeof UserSchema>;

/* ---------------- User Edit Schema ---------------- */
const UserEditSchema = UserSchema.omit({ email: true, userPass: true });
type UserEditSchema = z.infer<typeof UserEditSchema>;

/* ---------------- User Invite Schema ---------------- */
const UserInviteSchema = UserSchema.pick({ email: true });
type UserInviteSchema = z.infer<typeof UserInviteSchema>;

export { UserEditSchema, UserInviteSchema, UserSchema };
