export interface AudioPlaylistItem {
  id: string;
  title: string;
  audioUrl: string;
  duration: number | null;
  stepName: string;
  interviewId: string;
  candidateName: string;
}

import { env } from '../config/env';

export const interviewAudioService = {
  /**
   * Fetch job application with interviews for a specific candidate and job post
   */
  async getJobApplicationWithInterviews(
    api: any,
    jobPostName: string,
    jobPostVersion: number,
    candidateId: string
  ) {
    try {
      const response = await api.get(
        `/jobapplication/jobpost/${jobPostName}/${jobPostVersion}/candidate/${candidateId}`
      );
      
      return (response as any)?.data || response;
    } catch (error) {
      console.error('Error fetching job application with interviews:', error);
      throw new Error('Failed to fetch interview data');
    }
  },

  /**
   * Create a playlist from the job application data
   */
  createPlaylist(applicationData: any, candidateName: string): AudioPlaylistItem[] {
    const playlist: AudioPlaylistItem[] = [];

    if (!applicationData?.steps || !Array.isArray(applicationData.steps)) {
      return playlist;
    }

    applicationData.steps.forEach((step: any) => {
      if (!step.interviews || !Array.isArray(step.interviews)) {
        return;
      }

      step.interviews.forEach((interview: any) => {
        if (interview.interviewAudioUrl) {
          playlist.push({
            id: `${step.step.id}-${interview.id}`,
            title: `${candidateName} - ${step.step.jobPostStepName}`,
            audioUrl: interview.interviewAudioUrl,
            duration: interview.duration,
            stepName: step.step.jobPostStepName,
            interviewId: interview.id,
            candidateName: candidateName,
          });
        }
      });
    });

    return playlist;
  },

  /**
   * Fetch audio blob from ElevenLabs API with authentication
   */
  async fetchAudioBlob(audioUrl: string, tokenProvider: () => Promise<string | null>): Promise<Blob> {
    if (!audioUrl) {
      throw new Error('Audio URL is required');
    }

    try {
      const { resolvedUrl, requiresAuth } = resolveAudioEndpoint(audioUrl);
      const headers = new Headers();
      headers.append('Accept', 'audio/mpeg');

      if (requiresAuth) {
        const token = await tokenProvider();
        if (!token) {
          throw new Error('Authentication required to download interview audio.');
        }
        headers.append('Authorization', `Bearer ${token}`);
      }

      const response = await fetch(resolvedUrl, {
        method: 'GET',
        headers,
      });

      if (!response.ok) {
        throw new Error(`Failed to fetch audio: ${response.status} ${response.statusText}`);
      }

      return await response.blob();
    } catch (error) {
      console.error('Error fetching audio blob:', error);
      throw new Error('Failed to fetch audio file');
    }
  },
};

function resolveAudioEndpoint(audioUrl: string): { resolvedUrl: string; requiresAuth: boolean } {
  if (audioUrl.startsWith('http')) {
    if (audioUrl.includes('api.elevenlabs.io')) {
      const match = audioUrl.match(/conversations\/([^/]+)\/audio/);
      if (!match || match.length < 2) {
        throw new Error('Unrecognized ElevenLabs audio URL format.');
      }
      const conversationId = match[1];
      const baseApiUrl = normalisedApiBase();
      return {
        resolvedUrl: `${baseApiUrl}/candidate/ai-interview/conversations/${conversationId}/audio`,
        requiresAuth: true,
      };
    }

    try {
      const url = new URL(audioUrl);
      const origin = baseOrigin();
      const originUrl = new URL(origin);
      const requiresAuth = url.host === originUrl.host;
      return { resolvedUrl: audioUrl, requiresAuth };
    } catch {
      return { resolvedUrl: audioUrl, requiresAuth: false };
    }
  }

  const baseApiUrl = normalisedApiBase();

  if (audioUrl.startsWith('/api/')) {
    return {
      resolvedUrl: `${baseOrigin()}` + audioUrl,
      requiresAuth: true,
    };
  }

  if (audioUrl.startsWith('/')) {
    return {
      resolvedUrl: `${baseApiUrl}${audioUrl}`,
      requiresAuth: true,
    };
  }

  return {
    resolvedUrl: `${baseApiUrl}/${audioUrl}`,
    requiresAuth: true,
  };
}

function normalisedApiBase(): string {
  const apiBase = env.apiBaseUrl.replace(/\/+$/, '');
  return apiBase.endsWith('/api') ? apiBase : `${apiBase}/api`;
}

function baseOrigin(): string {
  const apiBase = env.apiBaseUrl.replace(/\/+$/, '');
  return apiBase.endsWith('/api') ? apiBase.slice(0, -4) : apiBase;
}
