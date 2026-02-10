"use client";

import { Pause, Play } from "lucide-react";
import { useEffect, useRef, useState } from "react";

import {
  AudioVisualizerCanvas,
  AudioVisualizerHover,
  AudioVisualizerProgress,
  AudioVisualizerRoot,
  AudioVisualizerTime,
} from "@/components/lib/AudioVisualizer";
import { Button } from "@/components/ui/button";
import { useIsBreakpoint } from "@/hooks/useBreakpoint";

const AUDIO_URL = "/demo-audio.mp3"; // Replace with your actual audio URL

export default function InterviewAudioDisplay() {
  const isMdOrLarger = useIsBreakpoint("md");
  const isXsOrLarger = useIsBreakpoint("xs");

  const [audioBlob, setAudioBlob] = useState<Blob | undefined>();
  const [currentTime, setCurrentTime] = useState(0);
  const [isPlaying, setIsPlaying] = useState(false);
  const [duration, setDuration] = useState(0);
  const audioRef = useRef<HTMLAudioElement>(null);

  const togglePlayback = () => {
    if (!audioRef.current) return;

    if (isPlaying) {
      audioRef.current.pause();
    } else {
      audioRef.current.play();
    }
    setIsPlaying(!isPlaying);
  };

  const handleTimeUpdate = () => {
    if (audioRef.current) {
      setCurrentTime(audioRef.current.currentTime);
    }
  };

  const handleEnded = () => {
    setIsPlaying(false);
  };

  const handlePlay = () => {
    setIsPlaying(true);
  };

  const handlePause = () => {
    setIsPlaying(false);
  };

  const handleLoadedMetadata = () => {
    if (audioRef.current) {
      setDuration(audioRef.current.duration);
    }
  };

  const handleSeek = (time: number) => {
    if (audioRef.current) {
      audioRef.current.currentTime = time;
      setCurrentTime(time);
    }
  };

  useEffect(() => {
    const fetchAudioBlob = async () => {
      try {
        const response = await fetch(AUDIO_URL);
        if (!response.ok) {
          throw new Error("Network response was not ok");
        }
        const audioBlob = await response.blob();
        setAudioBlob(audioBlob);
      } catch (error) {
        console.error("Failed to fetch audio blob:", error);
      }
    };

    fetchAudioBlob();

    // Cleanup
    return () => {
      if (audioRef.current) {
        // remove event listeners
        audioRef.current.removeEventListener("timeupdate", handleTimeUpdate);
        audioRef.current.removeEventListener("ended", handleEnded);
        audioRef.current.removeEventListener("play", handlePlay);
        audioRef.current.removeEventListener("pause", handlePause);
        audioRef.current.removeEventListener(
          "loadedmetadata",
          handleLoadedMetadata,
        );

        // Pause and reset audio ref
        audioRef.current.pause();
        audioRef.current = null;
      }
    };
  }, []);

  useEffect(() => {
    if (!audioBlob) return;

    const audio = new Audio(URL.createObjectURL(audioBlob));
    audioRef.current = audio;
    audio.addEventListener("timeupdate", handleTimeUpdate);
    audio.addEventListener("ended", handleEnded);
    audio.addEventListener("play", handlePlay);
    audio.addEventListener("pause", handlePause);
    audio.addEventListener("loadedmetadata", handleLoadedMetadata);
  }, [audioBlob]);

  return (
    <div className="container space-y-3">
      <div className="flex items-center justify-center gap-3">
        <Button onClick={togglePlayback} size="icon" variant="outline">
          {isPlaying ? (
            <Pause className="h-3 w-3" />
          ) : (
            <Play className="h-3 w-3" />
          )}
        </Button>

        <AudioVisualizerRoot
          blob={audioBlob}
          width={isMdOrLarger ? 600 : isXsOrLarger ? 300 : 200}
          height={75}
          barWidth={isMdOrLarger ? 2 : 1}
          gap={2}
          currentTime={currentTime}
          onSeek={handleSeek}
        >
          <AudioVisualizerCanvas />
          <AudioVisualizerProgress />
          <AudioVisualizerHover />
        </AudioVisualizerRoot>
      </div>

      <AudioVisualizerTime
        currentTime={currentTime}
        duration={duration}
        className="text-center text-xs"
      />
    </div>
  );
}
