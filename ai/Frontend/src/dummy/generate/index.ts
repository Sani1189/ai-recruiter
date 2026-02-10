import { writeFileSync } from "fs";
import { resolve } from "path";

import { generateModelData } from "./generate.model";
import { generateViewLayer } from "./generate.view";

/* ========= VIEW/MODEL LAYER PRINTING ========= */
const modelData = generateModelData(3, 10); // from previous script
const viewData = generateViewLayer(
  modelData.jobPosts,
  modelData.users,
  modelData.candidates,
  modelData.interviews,
);

function printModelData() {
  const MODEL_FILE_PATH = "../model.ts";

  const { jobPosts, users, candidates, interviews, notifications } = modelData;

  const interviewTs = `
    import {
      Candidate,
      Interview,
      JobPost,
      UserNotification,
      UserProfile,
    } from "@/types/v2/type";

    const MODEL: {
      JOB_POSTS: JobPost[];
      USER_PROFILES: UserProfile[];
      NOTIFICATIONS: UserNotification[];
      INTERVIEWS: Interview[];
      CANDIDATES: Candidate[];
    } = {
      JOB_POSTS: ${JSON.stringify(jobPosts, null, 2)},
      USER_PROFILES: ${JSON.stringify(users, null, 2)},
      NOTIFICATIONS: ${JSON.stringify(notifications, null, 2)},
      INTERVIEWS: ${JSON.stringify(interviews, null, 2)},
      CANDIDATES: ${JSON.stringify(candidates, null, 2)},
    };
  
    export default MODEL;
    `;

  const targetFilePath = resolve(__dirname, MODEL_FILE_PATH);
  // Write to model file
  writeFileSync(targetFilePath, interviewTs, "utf-8");

  console.log("✅ Model data written to:", targetFilePath);
}

function printViewData() {
  const VIEW_FILE_PATH = "../view.ts";

  const { candidateViews: candidates, interviewViews: interviews } = viewData;
  const { jobPosts, users, notifications } = modelData;

  const interviewTs = `
    import {
      CandidateView,
      InterviewView,
      JobPost,
      UserNotification,
      UserProfile,
    } from "@/types/v2/type.view";

    const VIEW: {
      JOB_POSTS: JobPost[];
      USER_PROFILES: UserProfile[];
      NOTIFICATIONS: UserNotification[];
      INTERVIEWS: InterviewView[];
      CANDIDATES: CandidateView[];
    } = {
      JOB_POSTS: ${JSON.stringify(jobPosts, null, 2)},
      USER_PROFILES: ${JSON.stringify(users, null, 2)},
      NOTIFICATIONS: ${JSON.stringify(notifications, null, 2)},
      INTERVIEWS: ${JSON.stringify(interviews, null, 2)},
      CANDIDATES: ${JSON.stringify(candidates, null, 2)},
    };
  
    export default VIEW;
    `;

  const targetFilePath = resolve(__dirname, VIEW_FILE_PATH);
  // Write to view file
  writeFileSync(targetFilePath, interviewTs, "utf-8");

  console.log("✅ View data written to:", targetFilePath);
}

function print() {
  if (process.argv.includes("--model")) {
    printModelData();
  } else {
    printViewData();
  }
}

print();
