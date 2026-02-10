# AI Interview System Implementation

## Problem Statement

The ElevenLabs conversational AI interview system was experiencing two critical issues:

1. **AI Silence**: The AI interviewer would sometimes go silent, leaving candidates without responses
2. **Duration Overrun**: Interview sessions would continue past the allocated time limit

## Solution: Three-Layer System

### Layer 1: Backend Prompt Generation (C#)

**Location:** `ElevenLabsConversationPayloadBuilder.cs`

The backend generates a comprehensive prompt that includes:

- **Behavioral Rules**: "NEVER REMAIN SILENT" directive to prevent silence issues
- **Duration Control**: Instructions for handling `[WRAP_UP_NOW]` commands
- **Personalized Greeting**: First message includes candidate name from authenticated user
- **Interview Structure**: Complete interview flow and guidelines

The prompt is returned as `SessionPayload` (JSON) to the frontend, centralizing all prompt logic in the backend.

### Layer 2: Frontend Real-Time Management (TypeScript)

**Location:** `useInterviewer.ts` hook

The frontend manages the interview session in real-time:

- **Uses Backend Prompt**: No frontend prompt building - uses `SessionPayload` from backend
- **Wrap-Up Timer**: Automatically sends `[WRAP_UP_NOW]` command 3 minutes before interview end
- **End Timer**: Forcefully closes the session when duration limit is reached
- **Heartbeat Check**: Monitors AI silence every 10 seconds; if silent >25s, sends nudge message

The application maintains full control over session termination, ensuring interviews end on time.

### Layer 3: Platform Settings (ElevenLabs)

**Configuration:**
- Max Time Limit: 3600s (1 hour) - Higher than interview duration
- End conversation after silence: 60s - Final failsafe if heartbeat check fails

## Results

✅ **No more silence issues** - Behavioral rules + heartbeat monitoring prevent AI silence  
✅ **Sessions end on time** - Application-controlled termination at duration limit  
✅ **Graceful wrap-up** - AI receives warning 3 minutes before end for smooth conclusion  
✅ **Maintainable architecture** - All prompt logic centralized in backend