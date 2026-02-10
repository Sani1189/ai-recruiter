
import { ApiResponse } from "@/lib/api/config";
import { InterviewConfigurationWithPrompts } from "@/types/interviewConfiguration";

type ApiClientLike = {
  post: <T = any>(
    url: string,
    body?: any,
    options?: Record<string, unknown>
  ) => Promise<ApiResponse<T> | T>;
};

type ConversationTokenResponse = {
  token: string;
  conversationId?: string | null;
  agentId?: string | null;
  sessionPayload?: string | null; // JSON string of agent_config_override from backend
};

type ConversationTokenPayload = {
  agentId?: string;
  interviewConfigurationName?: string;
  interviewConfigurationVersion?: number;
  jobPostName?: string;
  jobPostVersion?: number;
  jobApplicationId?: string;
  stepName?: string;
  stepDisplayTitle?: string;
};

function extractResponse<T>(response: ApiResponse<T> | T): T {
  if (typeof response === "object" && response !== null && "data" in response) {
    return (response as ApiResponse<T>).data;
  }
  return response as T;
}

export async function generateConversationTokenToken(
  api: ApiClientLike,
  payload: ConversationTokenPayload = {}
): Promise<ConversationTokenResponse> {
  const response = await api.post<ConversationTokenResponse>(
    "/candidate/ai-interview/token",
    payload
  );
  return extractResponse<ConversationTokenResponse>(response);
}

export async function generateDynamicConversationToken(
  api: ApiClientLike,
  interviewConfig: InterviewConfigurationWithPrompts,
  jobPost?: any,
  stepName?: string
) {
  try {
    const request: ConversationTokenPayload = {
      interviewConfigurationName: interviewConfig?.name,
      interviewConfigurationVersion: interviewConfig?.version,
      jobPostName: jobPost?.name,
      jobPostVersion: jobPost?.version,
      stepName: stepName,
    };

    return await generateConversationTokenToken(api, request);
  } catch (error) {
    console.error("Dynamic conversation token generation failed, falling back.", error);
    return generateConversationTokenToken(api);
  }
}
