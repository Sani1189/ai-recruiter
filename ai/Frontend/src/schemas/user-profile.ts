import { z } from "zod";

const UserProfileSchema = z.object({
  name: z.string().trim().nonempty("Name is required"),
  email: z.email("Invalid email format"),
  phoneNumber: z.string().trim().nonempty("Phone number is required"),
  age: z
    .number()
    .int()
    .min(18, "Age must be at least 18")
    .max(100, "Age must be at most 100"),
  nationality: z.string().trim().nonempty("Nationality is required"),
  profilePictureUrl: z.url("Invalid URL format"),
  bio: z
    .string()
    .trim()
    .min(10, "Bio must be at least 10 characters")
    .max(1000, "Bio must be at most 1000 characters"),
  jobTypePreferences: z
    .array(z.enum(["full-time", "part-time", "internship", "contract"]))
    .nonempty("At least one job type preference is required"),
  openToRelocation: z.boolean(),
  remotePreferences: z
    .array(z.enum(["remote", "hybrid", "on-site"]))
    .nonempty("At least one remote preference is required"),
  education: z
    .array(
      z.object({
        degree: z
          .string()
          .trim()
          .nonempty("Degree is required")
          .max(100, "Degree must be at most 100 characters"),
        fieldOfStudy: z
          .string()
          .trim()
          .nonempty("Field of study is required")
          .max(100, "Field of study must be at most 100 characters"),
        institution: z
          .string()
          .trim()
          .nonempty("Institution is required")
          .max(200, "Institution must be at most 200 characters"),
        graduationYear: z
          .number()
          .int()
          .min(1900)
          .max(new Date().getFullYear() + 10),
      }),
    )
    .nonempty("At least one education entry is required"),
  workExperience: z.array(
    z.object({
      company: z
        .string()
        .trim()
        .nonempty("Company is required")
        .max(100, "Company must be at most 100 characters"),
      role: z
        .string()
        .trim()
        .nonempty("Role is required")
        .max(100, "Role must be at most 100 characters"),
      startDate: z.iso.datetime(),
      endDate: z.iso.datetime(),
      responsibilities: z
        .string()
        .trim()
        .min(5, "Responsibility must be at least 5 characters")
        .max(1000, "Responsibility must be at most 1000 characters"),
    }),
  ),
  projects: z.array(
    z.object({
      title: z
        .string()
        .trim()
        .nonempty("Project title is required")
        .max(100, "Title must be at most 100 characters"),
      description: z
        .string()
        .trim()
        .min(10, "Description must be at least 10 characters")
        .max(1000, "Description must be at most 1000 characters"),
      url: z.url("Invalid URL format"),
      technologies: z
        .array(
          z.enum([
            "JavaScript",
            "Python",
            "Java",
            "C#",
            "Ruby",
            "Go",
            "PHP",
            "Swift",
            "Kotlin",
            "TypeScript",
            "Rust",
            "Dart",
            "Scala",
            "Elixir",
            "C++",
            "C",
            "HTML",
            "CSS",
            "SQL",
            "NoSQL",
            "GraphQL",
            "Docker",
            "Kubernetes",
            "AWS",
            "Azure",
            "GCP",
            "Firebase",
            "PostgreSQL",
            "MongoDB",
            "Redis",
            "MySQL",
            "SQLite",
          ]),
        )
        .nonempty("At least one technology is required"),
    }),
  ),
  speakingLanguages: z
    .array(
      z.object({
        language: z
          .string()
          .trim()
          .nonempty("Language is required")
          .max(50, "Language must be at most 50 characters"),
        proficiency: z.enum(["beginner", "intermediate", "advanced"]),
      }),
    )
    .nonempty("At least one speaking language is required"),
  programmingLanguages: z
    .array(
      z.object({
        language: z.enum(["JavaScript", "Python", "Java", "C#", "Ruby", "Go"]),
        proficiency: z.enum(["beginner", "intermediate", "advanced"]),
      }),
    )
    .nonempty("At least one programming language is required"),
  createdAt: z.iso.datetime(),
});
export type UserProfileSchema = z.infer<typeof UserProfileSchema>;

export { UserProfileSchema };
