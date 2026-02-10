import { z } from "zod";

const SignInForm = z.object({
  email: z.email("Invalid email address"),
  password: z.string().trim().nonempty("Password is required"),
});
type SignInForm = z.infer<typeof SignInForm>;

export { SignInForm };
