import { faker } from "@faker-js/faker";

import { CandidateNotificationType } from "@/enums";
import JobPostCreationForm from "@/schemas/job-posting";
import { UserProfileSchema } from "@/schemas/user-profile";
import {
  Candidate,
  Feedback,
  Interview,
  JobPost,
  Score,
  UserNotification,
  UserProfile,
} from "@/types/v2/type";

/* ========= Helpers ========= */

const fromArray = (
  { min, max }: { min: number; max: number },
  fn: () => any,
) => {
  return Array.from({ length: faker.number.int({ min, max }) }, fn);
};

function randomArrayElement<T>(arr: T[]): T {
  return arr[Math.floor(Math.random() * arr.length)];
}

function randomSubset<T>(arr: T[], min = 1, max = arr.length): T[] {
  const shuffled = [...arr].sort(() => 0.5 - Math.random());
  return shuffled.slice(0, faker.number.int({ min, max }));
}

/* ========= Generators ========= */

function generateJobPost(): JobPost {
  const jobType = faker.helpers.enumValue(
    JobPostCreationForm.shape.jobType.enum,
  );
  const experienceLevel = faker.helpers.enumValue(
    JobPostCreationForm.shape.experienceLevel.enum,
  );
  const jobTitle = `${experienceLevel} Level ${jobType}`;
  const toneOptions =
    ((JobPostCreationForm.shape as any).tone?.enum as string[] | undefined) ?? [
      "neutral",
      "friendly",
      "professional",
    ];
  const probingDepthOptions =
    ((JobPostCreationForm.shape as any).probingDepth?.enum as string[] | undefined) ?? [
      "surface",
      "moderate",
      "deep",
    ];
  const focusAreaOptions =
    ((JobPostCreationForm.shape as any).focusArea?.enum as string[] | undefined) ?? [
      "technical",
      "behavioral",
      "cultural",
    ];

  return {
    id: faker.string.alphanumeric({
      length: 10,
      casing: "lower",
    }),
    jobTitle,
    jobType,
    jobDescription: faker.lorem.paragraphs(10),
    experienceLevel,
    status: "Draft",
    countryExposureCountryCodes: [],
    tone: faker.helpers.arrayElement(toneOptions),
    probingDepth: faker.helpers.arrayElement(probingDepthOptions),
    focusArea: faker.helpers.arrayElement(focusAreaOptions),
    duration: faker.number.int({ min: 20, max: 60, multipleOf: 5 }),
    minimumRequirements: fromArray({ min: 0, max: 5 }, () =>
      faker.lorem.sentence({ min: 5, max: 10 }),
    ),
    instructions: faker.lorem.sentences({ min: 2, max: 4 }),
    policeReportRequired: faker.datatype.boolean(),
    createdAt: faker.date.recent({ days: 30 }).toISOString(),
    candidatesCount: 0, // update later
    interviewSteps: fromArray({ min: 1, max: 5 }, () => faker.lorem.word()),
    maxAmountOfCandidatesRestriction: faker.number.int({ min: 10, max: 500 }),
    steps: [
      {
        stepNumber: 1,
        existingStepName: "defaultStep",
        useLatestVersion: true,
        existingStepVersion: 1,
        displayName: "Default Step",
        displayVersion: "v1",
        stepType: "default",
        isInterview: false,
      },
    ],
    confirmed: true,
  };
}

function generateUserProfile(): UserProfile {
  const firstName = faker.person.firstName();
  const lastName = faker.person.lastName();

  return {
    id: faker.string.alphanumeric({
      length: 10,
      casing: "lower",
    }),
    name: `${firstName} ${lastName}`,
    email: faker.internet.email({ firstName, lastName }).toLowerCase(),
    phoneNumber: faker.phone.number({ style: "international" }),
    age: faker.number.int({ min: 22, max: 55 }),
    nationality: faker.location.country(),
    createdAt: faker.date.recent({ days: 30 }).toISOString(),
    bio: faker.lorem.paragraphs(3, "\n\n"),
    education: fromArray({ min: 2, max: 3 }, () => ({
      degree: faker.lorem.words({ min: 2, max: 4 }),
      graduationYear: faker.date.past({ years: 10 }).getFullYear(),
      fieldOfStudy: faker.lorem.words({ min: 2, max: 4 }),
      institution: faker.company.name(),
    })),
    jobTypePreferences: fromArray({ min: 1, max: 3 }, () =>
      faker.helpers.enumValue(
        UserProfileSchema.shape.jobTypePreferences.unwrap().enum,
      ),
    ),
    openToRelocation: faker.datatype.boolean(),
    profilePictureUrl: faker.image.avatar(),
    programmingLanguages: fromArray({ min: 1, max: 5 }, () => ({
      language: faker.helpers.enumValue(
        UserProfileSchema.shape.programmingLanguages.unwrap().shape.language
          .enum,
      ),
      proficiency: faker.helpers.enumValue(
        UserProfileSchema.shape.programmingLanguages.unwrap().shape.proficiency
          .enum,
      ),
    })),
    projects: fromArray({ min: 1, max: 3 }, () => ({
      title: faker.lorem.words({ min: 2, max: 5 }),
      description: faker.lorem.paragraphs({ min: 1, max: 2 }),
      technologies: fromArray({ min: 1, max: 3 }, () =>
        faker.helpers.enumValue(
          UserProfileSchema.shape.projects.unwrap().shape.technologies.unwrap()
            .enum,
        ),
      ),
      url: faker.internet.url(),
    })),
    remotePreferences: fromArray({ min: 1, max: 3 }, () =>
      faker.helpers.enumValue(
        UserProfileSchema.shape.remotePreferences.unwrap().enum,
      ),
    ),
    speakingLanguages: fromArray({ min: 1, max: 3 }, () => ({
      language: faker.lorem.word(),
      proficiency: faker.helpers.enumValue(
        UserProfileSchema.shape.speakingLanguages.unwrap().shape.proficiency
          .enum,
      ),
    })),
    workExperience: fromArray({ min: 2, max: 5 }, () => ({
      company: faker.company.name(),
      role: faker.lorem.words({ min: 2, max: 4 }),
      startDate: faker.date.past({ years: 10 }).toISOString(),
      endDate: faker.date.recent({ days: 30 }).toISOString(),
      responsibilities: faker.lorem.sentence({ min: 5, max: 10 }),
    })),
    resumeUrl: faker.internet.url() + "/resume.pdf",
  };
}

function generateInterview(jobPost: JobPost, candidate: Candidate): Interview {
  const stepsCount = Math.max(1, jobPost.interviewSteps?.length ?? 1);
  return {
    id: faker.string.uuid(),
    jobPostId: jobPost.id,
    candidateId: candidate.id,
    score: generateScore(),
    feedback: generateFeedback(),
    interviewAudioUrl: faker.internet.url(),
    interviewQuestions: Array.from(
      { length: faker.number.int({ min: 3, max: 6 }) },
      () => faker.lorem.sentence(),
    ),
    completedAt: faker.date.recent().toISOString(),
    duration: faker.number.int({ min: 20, max: 90 }),
    currentStepIndex: faker.number.int({
      min: 0,
      max: stepsCount - 1,
    }),
  };
}

function generateScore(): Score {
  const english = faker.number.int({ min: 1, max: 10 });
  const technical = faker.number.int({ min: 1, max: 10 });
  const communication = faker.number.int({ min: 1, max: 10 });
  const problemSolving = faker.number.int({ min: 1, max: 10 });
  const avg = Math.round(
    (english + technical + communication + problemSolving) / 4,
  );
  return { avg, english, technical, communication, problemSolving };
}

function generateFeedback(): Feedback {
  return {
    detailed: faker.lorem.paragraphs(2),
    summary: faker.lorem.sentence(),
    strengths: randomSubset(
      [
        "Communication",
        "Teamwork",
        "Problem Solving",
        "Technical Knowledge",
        "Creativity",
      ],
      1,
      3,
    ),
    weaknesses: randomSubset(
      ["Time Management", "Adaptability", "Technical Gaps", "Overthinking"],
      1,
      2,
    ),
  };
}

function generateUserNotification(): UserNotification {
  return {
    id: faker.string.uuid(),
    type: faker.helpers.enumValue(CandidateNotificationType),
    title: faker.lorem.sentence({ min: 3, max: 6 }),
    message: faker.lorem.paragraph({ min: 2, max: 4 }),
    timestamp: faker.date.recent({ days: 30 }).toISOString(),
    read: faker.datatype.boolean(),
    actionUrl: faker.datatype.boolean() ? faker.internet.url() : undefined,
  };
}

/* ========= Data Linking ========= */

function generateModelData(
  jobPostCount = 3,
  userCount = 10,
  notificationCount = 15,
) {
  const jobPosts: JobPost[] = Array.from(
    { length: jobPostCount },
    generateJobPost,
  );
  const users: UserProfile[] = Array.from(
    { length: userCount },
    generateUserProfile,
  );
  const notifications: UserNotification[] = Array.from(
    { length: notificationCount },
    generateUserNotification,
  );

  const candidates: Candidate[] = [];
  const interviews: Interview[] = [];

  // Assign candidates to job posts
  jobPosts.forEach((jobPost) => {
    const candidatePool = faker.helpers.arrayElements(
      users,
      faker.number.int({ min: 1, max: userCount }),
    );
    candidatePool.forEach((user) => {
      const candidate: Candidate = { id: faker.string.uuid(), userId: user.id };
      candidates.push(candidate);

      // Create exactly one interview per (Candidate + JobPost)
      const interview: Interview = generateInterview(jobPost, candidate);
      interviews.push(interview);
    });
    jobPost.candidatesCount = candidatePool.length;
  });

  return { jobPosts, users, candidates, interviews, notifications };
}

export { generateModelData };
