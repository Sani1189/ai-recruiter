"use client";

import { Download, Pause, Play, SkipBack, SkipForward, Volume2, VolumeX } from "lucide-react";
import { useEffect, useRef, useState } from "react";

import {
  AudioVisualizerCanvas,
  AudioVisualizerHover,
  AudioVisualizerProgress,
  AudioVisualizerRoot,
  AudioVisualizerTime,
} from "@/components/lib/AudioVisualizer";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Slider } from "@/components/ui/slider";
import { useIsBreakpoint } from "@/hooks/useBreakpoint";
import { AudioPlaylistItem, interviewAudioService } from "@/lib/services/interviewAudioService";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";

interface InterviewAudioPlayerProps {
  playlist: AudioPlaylistItem[];
  candidateName?: string;
}

export default function InterviewAudioPlayer({ 
  playlist, 
  candidateName 
}: InterviewAudioPlayerProps) {
  const { getAccessToken } = useUnifiedAuth();
  const isMdOrLarger = useIsBreakpoint("md");
  const isXsOrLarger = useIsBreakpoint("xs");

  const [currentTrackIndex, setCurrentTrackIndex] = useState(0);
  const [audioBlob, setAudioBlob] = useState<Blob | undefined>();
  const [currentTime, setCurrentTime] = useState(0);
  const [isPlaying, setIsPlaying] = useState(false);
  const [duration, setDuration] = useState(0);
  const [volume, setVolume] = useState(1);
  const [isMuted, setIsMuted] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const audioRef = useRef<HTMLAudioElement>(null);

  const currentTrack = playlist[currentTrackIndex];

  // Load audio for current track
  useEffect(() => {
    if (!currentTrack) return;

    const loadAudio = async () => {
      setIsLoading(true);
      setError(null);
      
      try {
        const blob = await interviewAudioService.fetchAudioBlob(
          currentTrack.audioUrl,
          getAccessToken
        );
        setAudioBlob(blob);
        setCurrentTime(0);
        setIsPlaying(false);
      } catch (err) {
        console.error('Error loading audio:', err);
        setError(err instanceof Error ? err.message : 'Failed to load audio');
        setAudioBlob(undefined);
      } finally {
        setIsLoading(false);
      }
    };

    loadAudio();
  }, [currentTrackIndex, currentTrack]);

  // Setup audio element
  useEffect(() => {
    if (!audioBlob) return;

    const audio = new Audio(URL.createObjectURL(audioBlob));
    audioRef.current = audio;
    
    const handleTimeUpdate = () => {
      setCurrentTime(audio.currentTime);
    };

    const handleEnded = () => {
      setIsPlaying(false);
      // Auto-play next track if available
      if (currentTrackIndex < playlist.length - 1) {
        setCurrentTrackIndex(currentTrackIndex + 1);
      }
    };

    const handlePlay = () => {
      setIsPlaying(true);
    };

    const handlePause = () => {
      setIsPlaying(false);
    };

    const handleLoadedMetadata = () => {
      setDuration(audio.duration);
    };

    audio.addEventListener("timeupdate", handleTimeUpdate);
    audio.addEventListener("ended", handleEnded);
    audio.addEventListener("play", handlePlay);
    audio.addEventListener("pause", handlePause);
    audio.addEventListener("loadedmetadata", handleLoadedMetadata);

    // Cleanup
    return () => {
      audio.removeEventListener("timeupdate", handleTimeUpdate);
      audio.removeEventListener("ended", handleEnded);
      audio.removeEventListener("play", handlePlay);
      audio.removeEventListener("pause", handlePause);
      audio.removeEventListener("loadedmetadata", handleLoadedMetadata);
      audio.pause();
      URL.revokeObjectURL(audio.src);
    };
  }, [audioBlob, currentTrackIndex, playlist.length]);

  // Update volume
  useEffect(() => {
    if (audioRef.current) {
      audioRef.current.volume = isMuted ? 0 : volume;
    }
  }, [volume, isMuted]);

  const togglePlayback = () => {
    if (!audioRef.current) return;

    if (isPlaying) {
      audioRef.current.pause();
    } else {
      audioRef.current.play();
    }
  };

  const handleSeek = (time: number) => {
    if (audioRef.current) {
      audioRef.current.currentTime = time;
      setCurrentTime(time);
    }
  };

  const goToPreviousTrack = () => {
    if (currentTrackIndex > 0) {
      setCurrentTrackIndex(currentTrackIndex - 1);
    }
  };

  const goToNextTrack = () => {
    if (currentTrackIndex < playlist.length - 1) {
      setCurrentTrackIndex(currentTrackIndex + 1);
    }
  };

  const toggleMute = () => {
    setIsMuted(!isMuted);
  };

  const handleDownload = async () => {
    if (!audioBlob || !currentTrack) return;

    try {
      const safeTitle =
        currentTrack.title?.replace(/[^\w\s-]/g, "").replace(/\s+/g, "-").toLowerCase() ||
        currentTrack.stepName?.replace(/[^\w\s-]/g, "").replace(/\s+/g, "-").toLowerCase() ||
        currentTrack.id;

      const url = URL.createObjectURL(audioBlob);
      const anchor = document.createElement("a");
      anchor.href = url;
      anchor.download = `${safeTitle || "interview-audio"}.mp3`;
      document.body.appendChild(anchor);
      anchor.click();
      document.body.removeChild(anchor);
      setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch (err) {
      console.error("Failed to download audio", err);
      setError("Failed to download audio file");
    }
  };

  const formatTime = (time: number) => {
    const minutes = Math.floor(time / 60);
    const seconds = Math.floor(time % 60);
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  };

  if (playlist.length === 0) {
    return (
      <Card className="shadow-card">
        <CardContent className="pt-6">
          <div className="text-center text-muted-foreground">
            No interview audio available for {candidateName || 'this candidate'}.
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="shadow-card">
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <span>
            Interview Audio Playlist
            {candidateName && ` - ${candidateName}`}
          </span>
          <span className="text-sm font-normal text-muted-foreground">
            {currentTrackIndex + 1} of {playlist.length}
          </span>
        </CardTitle>
      </CardHeader>

      <CardContent className="space-y-6">
        {/* Current Track Info */}
        {currentTrack && (
          <div className="text-center space-y-2">
            <h3 className="font-semibold">{currentTrack.title}</h3>
            <p className="text-sm text-muted-foreground">
              {currentTrack.stepName}
            </p>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="text-center text-red-500 text-sm">
            {error}
          </div>
        )}

        {/* Loading State */}
        {isLoading && (
          <div className="text-center text-muted-foreground text-sm">
            Loading audio...
          </div>
        )}

        {/* Audio Controls */}
        {audioBlob && !error && (
          <>
            {/* Playback Controls */}
            <div className="flex items-center justify-center gap-4">
              <Button
                onClick={goToPreviousTrack}
                disabled={currentTrackIndex === 0}
                variant="ghost"
                size="sm"
              >
                <SkipBack className="h-4 w-4" />
              </Button>

              <Button
                onClick={togglePlayback}
                disabled={isLoading}
                size="lg"
                className="rounded-full w-12 h-12"
              >
                {isPlaying ? (
                  <Pause className="h-6 w-6" />
                ) : (
                  <Play className="h-6 w-6" />
                )}
              </Button>

              <Button
                onClick={goToNextTrack}
                disabled={currentTrackIndex === playlist.length - 1}
                variant="ghost"
                size="sm"
              >
                <SkipForward className="h-4 w-4" />
              </Button>

              <Button
                onClick={handleDownload}
                variant="ghost"
                size="sm"
                disabled={!audioBlob}
                title="Download audio"
              >
                <Download className="h-4 w-4" />
              </Button>
            </div>

            {/* Audio Visualizer */}
            <div className="flex items-center justify-center gap-3">
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

            {/* Time Display */}
            <AudioVisualizerTime
              currentTime={currentTime}
              duration={duration}
              className="text-center text-sm"
            />

            {/* Volume Control */}
            <div className="flex items-center justify-center gap-3">
              <Button
                onClick={toggleMute}
                variant="ghost"
                size="sm"
              >
                {isMuted ? (
                  <VolumeX className="h-4 w-4" />
                ) : (
                  <Volume2 className="h-4 w-4" />
                )}
              </Button>
              
              <div className="flex items-center gap-2 w-32">
                <Slider
                  value={[volume]}
                  onValueChange={([value]) => setVolume(value)}
                  max={1}
                  min={0}
                  step={0.1}
                  className="w-full"
                />
                <span className="text-xs text-muted-foreground w-8">
                  {Math.round(volume * 100)}
                </span>
              </div>
            </div>
          </>
        )}

        {/* Playlist */}
        {playlist.length > 1 && (
          <div className="space-y-2">
            <h4 className="font-medium text-sm">Playlist</h4>
            <div className="max-h-40 overflow-y-auto space-y-1">
              {playlist.map((track, index) => (
                <div
                  key={track.id}
                  className={`flex items-center justify-between p-2 rounded cursor-pointer transition-colors ${
                    index === currentTrackIndex
                      ? 'bg-primary/10 border border-primary/20'
                      : 'hover:bg-muted'
                  }`}
                  onClick={() => setCurrentTrackIndex(index)}
                >
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium truncate">
                      {track.title}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {track.stepName}
                    </p>
                  </div>
                  {track.duration && (
                    <span className="text-xs text-muted-foreground ml-2">
                      {formatTime(track.duration * 60)}
                    </span>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
