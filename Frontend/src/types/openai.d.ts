type DefaultResponseTranscript = {
  event_id: string;
  item_id: string;
  content_index: number;
  transcript: string;
};

type UserResponseTranscript = DefaultResponseTranscript & {
  type: "conversation.item.input_audio_transcription.completed";
};

type AssistantResponseTranscript = DefaultResponseTranscript & {
  type: "response.audio_transcript.done";
  output_index: number;
  response_id: string;
};

type UserSpeechStart = Pick<
  DefaultResponseTranscript,
  "item_id" | "event_id"
> & {
  type: "input_audio_buffer.speech_started";
  audio_start_ms: number;
};

type UserSpeechEnd = Pick<DefaultResponseTranscript, "item_id" | "event_id"> & {
  type: "input_audio_buffer.speech_stopped";
  audio_end_ms: number;
};

type AssistantSpeechStart = Pick<DefaultResponseTranscript, "item_id"> & {
  type: "output_audio_buffer.started";
  response_id: string;
};

type AssistantSpeechEnd = Pick<DefaultResponseTranscript, "item_id"> & {
  type: "output_audio_buffer.stopped";
  response_id: string;
};

export type ExtendedTransportEvent =
  | AssistantSpeechStart
  | AssistantSpeechEnd
  | UserSpeechEnd
  | UserSpeechStart
  | UserResponseTranscript
  | AssistantResponseTranscript;
