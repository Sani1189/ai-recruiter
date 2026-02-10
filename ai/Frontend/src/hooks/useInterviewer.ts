import { useConversation } from "@elevenlabs/react";
import { useState, useEffect, useRef, useCallback } from "react";
import { InterviewConfigurationWithPrompts } from "@/types/interviewConfiguration";

enum CallStatus {
  INACTIVE = "INACTIVE",
  CONNECTING = "CONNECTING",
  ACTIVE = "ACTIVE",
  FINISHED = "FINISHED",
  MUTED = "MUTED",
}

type SessionPayload = {
  dynamic_variables?: Record<string, string>;
  conversation_initiation_client_data?: {
    dynamic_variables?: Record<string, string>;
  };
};

// Constants for interview timing and monitoring
const TIMING = {
  HEARTBEAT_CHECK_INTERVAL: 5000, // Check every 5 seconds
  HEARTBEAT_START_DELAY: 10000, // Wait 10s before starting heartbeat checks
  AI_SILENCE_THRESHOLD: 45000, // AI silent for 45 seconds triggers nudge (allows for longer user responses)
  SILENCE_AFTER_NUDGE_DELAY: 15000, // Wait 15s after nudge before checking again
  USER_DONE_SPEAKING_DELAY: 8000, // Consider user done speaking after 8s of silence
  WRAP_UP_SHORT: 1, // 2 minutes for interviews < 10 min
  WRAP_UP_MEDIUM: 2, // 3 minutes for interviews 10-14 min
  WRAP_UP_LONG: 3, // 4 minutes for interviews >= 15 min
} as const;

const useInterviewer = (
  conversationToken: string,
  interviewConfig?: InterviewConfigurationWithPrompts,
  jobPost?: any,
  sessionPayloadJson?: string | null,
  durationMinutes?: number
) => {
  const [callStatus, setCallStatus] = useState<CallStatus>(CallStatus.INACTIVE);
  const [messages, setMessages] = useState<string[]>([]);
  const [error, setError] = useState<Error | null>(null);
  const [isSpeaking, setIsSpeaking] = useState(false);
  const [conversationId, setConversationId] = useState<string | null>(null);
  const [showHelpMessage, setShowHelpMessage] = useState<boolean>(false);
  
  // Timer and state refs
  const wrapUpTimerRef = useRef<NodeJS.Timeout | null>(null);
  const endTimerRef = useRef<NodeJS.Timeout | null>(null);
  const heartbeatTimerRef = useRef<NodeJS.Timeout | null>(null);
  const lastActivityTimeRef = useRef<number>(Date.now());
  const lastNudgeTimeRef = useRef<number>(0);
  const lastUserSpeechTimeRef = useRef<number>(0);
  const interviewStartTimeRef = useRef<number | null>(null);
  const hasReceivedFirstMessageRef = useRef<boolean>(false);
  const wrapUpSentRef = useRef<boolean>(false);
  const isActiveRef = useRef<boolean>(false);

  const conversation = useConversation({
    connectionType: "webrtc",
    onMessage: ({ message, source }: any) => {
      setMessages((prev) => [...prev, message]);
      lastActivityTimeRef.current = Date.now();
      hasReceivedFirstMessageRef.current = true;
      
      // Track user speech to avoid nudging during active conversation
      if (source === "user") {
        lastUserSpeechTimeRef.current = Date.now();
      }
    },
    onDisconnect: () => {
      isActiveRef.current = false;
      setCallStatus(CallStatus.FINISHED);
      setIsSpeaking(false);
      clearAllTimers();
    },
    onError: (error) => {
      setError(new Error(error));
    },
  });

  // Keep ref in sync with callStatus
  useEffect(() => {
    isActiveRef.current = callStatus === CallStatus.ACTIVE;
  }, [callStatus]);

  // Track when AI is speaking
  useEffect(() => {
    if (callStatus !== CallStatus.ACTIVE) return;
    
    const conv = conversation as any;
    if (!conv?.isSpeaking) return;
    
    const checkInterval = setInterval(() => {
      const nowSpeaking = Boolean(conv.isSpeaking);
      
      if (nowSpeaking !== isSpeaking) {
        setIsSpeaking(nowSpeaking);
        if (nowSpeaking) {
          lastActivityTimeRef.current = Date.now();
        }
      }
    }, 500);
    
    return () => clearInterval(checkInterval);
  }, [conversation, callStatus, isSpeaking]);

  // Clear all timers helper
  const clearAllTimers = useCallback(() => {
    [wrapUpTimerRef, endTimerRef, heartbeatTimerRef].forEach((timerRef) => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
        clearInterval(timerRef.current);
        timerRef.current = null;
      }
    });
  }, []);

  // Send message to AI during ongoing conversation
  const sendMessage = useCallback((message: string) => {
    // Use ref to check active state to avoid stale closure issues
    if (!isActiveRef.current) {
      return;
    }

    try {
      const conv = conversation as any;
      const isWrapUpMessage = message.includes("[WRAP_UP_NOW]");
      
      // Try sendUserContext first for wrap-up messages (less disruptive to audio flow)
      // Otherwise try sendUserMessage, then fallback to sendUserContext
      if (isWrapUpMessage && conv?.sendUserContext) {
        conv.sendUserContext(message);
        lastActivityTimeRef.current = Date.now();
      } else if (conv?.sendUserMessage) {
        conv.sendUserMessage(message);
        lastActivityTimeRef.current = Date.now();
      } else if (conv?.sendUserContext) {
        conv.sendUserContext(message);
        lastActivityTimeRef.current = Date.now();
      } else {
        // Don't set error for wrap-up messages as they're not critical
        if (!isWrapUpMessage) {
          setError(new Error("Cannot send message - conversation method not available"));
        }
      }
    } catch (err) {
      // Only set error for non-wrap-up messages
      if (!message.includes("[WRAP_UP_NOW]")) {
        setError(err instanceof Error ? err : new Error(String(err)));
      }
    }
  }, [conversation]);

  // Send nudge to AI when conversation stalls
  const sendNudgeToAI = useCallback(() => {
    const now = Date.now();
    const timeSinceLastNudge = now - lastNudgeTimeRef.current;
    const timeSinceUserSpoke = now - lastUserSpeechTimeRef.current;
    
    // Don't nudge if nudged recently
    if (timeSinceLastNudge < TIMING.SILENCE_AFTER_NUDGE_DELAY) {
      return;
    }
    
    // Don't nudge if user was speaking recently (still thinking/responding)
    if (timeSinceUserSpoke < TIMING.USER_DONE_SPEAKING_DELAY) {
      return;
    }
    
    const silenceDuration = Math.round((now - lastActivityTimeRef.current) / 1000);
    lastNudgeTimeRef.current = now;
  }, [conversation]);

  const connect = async () => {
    setCallStatus(CallStatus.CONNECTING);
    clearAllTimers();
    hasReceivedFirstMessageRef.current = false;

    try {
      const permission = await navigator.mediaDevices.getUserMedia({
        audio: true,
      });
      
      if (!permission) {
        isActiveRef.current = false;
        setError(new Error("Microphone access denied"));
        setCallStatus(CallStatus.INACTIVE);
        return;
      }
      
      // Parse session payload from backend
      let sessionPayload: SessionPayload | undefined;
      let dynamicVariables: Record<string, string> | undefined;
      
      if (sessionPayloadJson) {
        try {
          sessionPayload = JSON.parse(sessionPayloadJson) as SessionPayload;
          dynamicVariables = sessionPayload.dynamic_variables || 
                           sessionPayload.conversation_initiation_client_data?.dynamic_variables;
        } catch (e) {
        }
      }

      // Build start payload - use backend-generated prompt
      const startPayload: any = {
        conversationToken,
      };

      if (dynamicVariables) {
        const combinedPrompt = dynamicVariables.combined_prompt || 
          Object.entries(dynamicVariables)
            .filter(([key]) => !['combined_prompt'].includes(key))
            .map(([key, value]) => `${key}: ${value}`)
            .join('\n');

        startPayload.conversationInitiationClientData = {
          dynamic_variables: dynamicVariables,
        };
        startPayload.dynamicVariables = dynamicVariables;
        startPayload.conversationConfigOverride = {
          agent: {
            prompt: { prompt: combinedPrompt },
            first_message: dynamicVariables.first_message,
          },
        };
      }

      // Start session
      let convId: string | undefined;
      try {
        convId = await (conversation as any).startSession(startPayload);
      } catch (e) {
        convId = await conversation.startSession({ conversationToken });
      }
      
      if (convId) {
        setConversationId(convId);
      }

      // Initialize timing
      const now = Date.now();
      interviewStartTimeRef.current = now;
      lastActivityTimeRef.current = now;
      lastNudgeTimeRef.current = 0;
      
      // Calculate interview duration and timing
      const interviewDuration = durationMinutes || 
        (dynamicVariables?.duration ? parseInt(dynamicVariables.duration) : 30);
      const durationMs = interviewDuration * 60 * 1000;
      
      // Calculate wrap-up time
      let wrapUpMinutes = TIMING.WRAP_UP_SHORT as number;
      if (interviewDuration >= 15) {
        wrapUpMinutes = TIMING.WRAP_UP_LONG;
      } else if (interviewDuration >= 10) {
        wrapUpMinutes = TIMING.WRAP_UP_MEDIUM;
      }
      const wrapUpMs = Math.max(0, durationMs - (wrapUpMinutes * 60 * 1000));

      // Set wrap-up timer to send wrap-up signal to AI
      if (wrapUpMs > 0) {
        wrapUpTimerRef.current = setTimeout(() => {
          wrapUpSentRef.current = true;
          sendMessage("[WRAP_UP_NOW]");
        }, wrapUpMs);
      }

      // Set end timer
      endTimerRef.current = setTimeout(() => {
        sendMessage("[THANKS_FOR_YOUR_TIME]");
        disconnect();
      }, durationMs);

      // Set up heartbeat monitor
      setTimeout(() => {
        heartbeatTimerRef.current = setInterval(() => {
          const conv = conversation as any;
          const aiIsSpeaking = Boolean(conv?.isSpeaking);
          const timeSinceActivity = Date.now() - lastActivityTimeRef.current;
          
          // Check if conversation has stalled
          if (!aiIsSpeaking && timeSinceActivity > TIMING.AI_SILENCE_THRESHOLD) {
            // Only send nudge if we've received at least one message
            if (hasReceivedFirstMessageRef.current) {
              sendNudgeToAI();
            } else {
            }
          }
        }, TIMING.HEARTBEAT_CHECK_INTERVAL);
      }, TIMING.HEARTBEAT_START_DELAY);
      
      isActiveRef.current = true;
      setCallStatus(CallStatus.ACTIVE);
      setShowHelpMessage(true);
      
    } catch (err) {
      isActiveRef.current = false;
      setError(err instanceof Error ? err : new Error(String(err)));
      setCallStatus(CallStatus.INACTIVE);
      clearAllTimers();
    }
  };

  const disconnect = async () => {
    isActiveRef.current = false;
    clearAllTimers();
    
    try {
      await conversation.endSession();
    } catch (err) {
    }
    
    setCallStatus(CallStatus.FINISHED);
    setIsSpeaking(false);
    interviewStartTimeRef.current = null;
    hasReceivedFirstMessageRef.current = false;
    wrapUpSentRef.current = false;
    lastUserSpeechTimeRef.current = 0;
  };

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      clearAllTimers();
    };
  }, [clearAllTimers]);

  return {
    callStatus,
    messages,
    isSpeaking,
    error,
    conversationId,
    showHelpMessage,
    action: {
      connect,
      disconnect,
      sendMessage,
      dismissHelpMessage: () => setShowHelpMessage(false),
    },
  };
};

export default useInterviewer;
export { CallStatus };