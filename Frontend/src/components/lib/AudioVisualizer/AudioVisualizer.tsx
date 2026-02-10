"use client";

import {
  createContext,
  forwardRef,
  useContext,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
  type ReactNode,
} from "react";

import { cn } from "@/lib/utils";
import type { dataPoint } from "./types";
import { calculateBarData, draw } from "./utils";

// Context for sharing state between compound components
interface AudioVisualizerContextType {
  blob?: Blob;
  width: number;
  height: number;
  barWidth: number;
  gap: number;
  backgroundColor: string;
  barColor: string;
  barPlayedColor: string;
  currentTime?: number;
  data: dataPoint[];
  duration: number;
  canvasRef: React.RefObject<HTMLCanvasElement | null>;
  onSeek?: (time: number) => void;
  hoverTime: number | null;
  hoverPosition: number | null;
  setHoverTime: (time: number | null) => void;
  setHoverPosition: (position: number | null) => void;
}

const AudioVisualizerContext = createContext<AudioVisualizerContextType | null>(
  null,
);

const useAudioVisualizer = () => {
  const context = useContext(AudioVisualizerContext);
  if (!context) {
    throw new Error(
      "AudioVisualizer compound components must be used within AudioVisualizerRoot",
    );
  }
  return context;
};

// Hook that safely tries to get context but doesn't throw if not available
const useAudioVisualizerSafe = () => {
  return useContext(AudioVisualizerContext);
};

// Root component that provides context
interface AudioVisualizerRootProps {
  children: ReactNode;
  blob?: Blob;
  width?: number;
  height?: number;
  barWidth?: number;
  gap?: number;
  backgroundColor?: string;
  barColor?: string;
  barPlayedColor?: string;
  currentTime?: number;
  className?: string;
  onSeek?: (time: number) => void;
}

const AudioVisualizerRoot = forwardRef<
  HTMLDivElement,
  AudioVisualizerRootProps
>(
  (
    {
      children,
      blob,
      width = 400,
      height = 100,
      barWidth = 2,
      gap = 1,
      backgroundColor = "transparent",
      barColor = "#ADADAD",
      barPlayedColor = "#0a0a0a",
      currentTime,
      className,
      onSeek,
      ...props
    },
    ref,
  ) => {
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const [data, setData] = useState<dataPoint[]>([]);
    const [duration, setDuration] = useState<number>(0);
    const [hoverTime, setHoverTime] = useState<number | null>(null);
    const [hoverPosition, setHoverPosition] = useState<number | null>(null);

    const contextValue: AudioVisualizerContextType = {
      blob,
      width,
      height,
      barWidth,
      gap,
      backgroundColor,
      barColor,
      barPlayedColor,
      currentTime,
      data,
      duration,
      canvasRef,
      onSeek,
      hoverTime,
      hoverPosition,
      setHoverTime,
      setHoverPosition,
    };

    useEffect(() => {
      const processBlob = async (): Promise<void> => {
        if (!canvasRef.current) return;

        if (!blob) {
          const barsData = Array.from({ length: 100 }, () => ({
            max: 0,
            min: 0,
          }));
          setData(barsData);
          draw(
            barsData,
            canvasRef.current,
            barWidth,
            gap,
            backgroundColor,
            barColor,
            barPlayedColor,
          );
          return;
        }

        try {
          const audioBuffer = await blob.arrayBuffer();
          const audioContext = new AudioContext();
          await audioContext.decodeAudioData(audioBuffer, (buffer) => {
            if (!canvasRef.current) return;
            setDuration(buffer.duration);
            const barsData = calculateBarData(
              buffer,
              height,
              width,
              barWidth,
              gap,
            );
            setData(barsData);
            draw(
              barsData,
              canvasRef.current,
              barWidth,
              gap,
              backgroundColor,
              barColor,
              barPlayedColor,
            );
          });
        } catch (error) {
          console.error("Error processing audio blob:", error);
          // Fallback to empty visualization
          const barsData = Array.from({ length: 100 }, () => ({
            max: 0,
            min: 0,
          }));
          setData(barsData);
          draw(
            barsData,
            canvasRef.current,
            barWidth,
            gap,
            backgroundColor,
            barColor,
            barPlayedColor,
          );
        }
      };

      processBlob();
    }, [
      blob,
      width,
      height,
      barWidth,
      gap,
      backgroundColor,
      barColor,
      barPlayedColor,
    ]);

    useEffect(() => {
      if (!canvasRef.current || data.length === 0) return;

      draw(
        data,
        canvasRef.current,
        barWidth,
        gap,
        backgroundColor,
        barColor,
        barPlayedColor,
        currentTime,
        duration,
      );
    }, [
      currentTime,
      duration,
      data,
      barWidth,
      gap,
      backgroundColor,
      barColor,
      barPlayedColor,
    ]);

    return (
      <AudioVisualizerContext.Provider value={contextValue}>
        <div ref={ref} className={cn("relative", className)} {...props}>
          {children}
        </div>
      </AudioVisualizerContext.Provider>
    );
  },
);
AudioVisualizerRoot.displayName = "AudioVisualizerRoot";

// Canvas component
interface AudioVisualizerCanvasProps {
  className?: string;
  style?: React.CSSProperties;
}

const AudioVisualizerCanvas = forwardRef<
  HTMLCanvasElement,
  AudioVisualizerCanvasProps
>(({ className, style, ...props }, ref) => {
  const {
    width,
    height,
    canvasRef,
    duration,
    onSeek,
    setHoverTime,
    setHoverPosition,
  } = useAudioVisualizer();

  useImperativeHandle<HTMLCanvasElement | null, HTMLCanvasElement | null>(
    ref,
    () => canvasRef.current,
    [],
  );

  const handleMouseMove = (event: React.MouseEvent<HTMLCanvasElement>) => {
    if (!canvasRef.current || duration <= 0) return;

    const rect = canvasRef.current.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const percentage = Math.max(0, Math.min(1, x / width));
    const time = percentage * duration;

    setHoverTime(time);
    setHoverPosition(x);
  };

  const handleMouseLeave = () => {
    setHoverTime(null);
    setHoverPosition(null);
  };

  const handleClick = (event: React.MouseEvent<HTMLCanvasElement>) => {
    if (!canvasRef.current || duration <= 0 || !onSeek) return;

    const rect = canvasRef.current.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const percentage = Math.max(0, Math.min(1, x / width));
    const time = percentage * duration;

    onSeek(time);
  };

  return (
    <canvas
      ref={canvasRef}
      width={width}
      height={height}
      className={cn("block cursor-pointer", className)}
      style={style}
      onMouseMove={handleMouseMove}
      onMouseLeave={handleMouseLeave}
      onClick={handleClick}
      {...props}
    />
  );
});
AudioVisualizerCanvas.displayName = "AudioVisualizerCanvas";

// Progress indicator component
interface AudioVisualizerProgressProps {
  className?: string;
}

const AudioVisualizerProgress = forwardRef<
  HTMLDivElement,
  AudioVisualizerProgressProps
>(({ className, ...props }, ref) => {
  const { currentTime = 0, duration, width } = useAudioVisualizer();

  const progressPercent = duration > 0 ? (currentTime / duration) * 100 : 0;
  const progress = (progressPercent / 100) * width;

  return (
    <div
      ref={ref}
      className={cn(
        "bg-primary pointer-events-none absolute top-0 h-full w-[0.19rem] rounded-full",
        className,
      )}
      style={{ left: progress }}
      {...props}
    />
  );
});
AudioVisualizerProgress.displayName = "AudioVisualizerProgress";

// Hover indicator component
interface AudioVisualizerHoverProps {
  className?: string;
}

const AudioVisualizerHover = forwardRef<
  HTMLDivElement,
  AudioVisualizerHoverProps
>(({ className, ...props }, ref) => {
  const { hoverPosition, hoverTime } = useAudioVisualizer();

  if (hoverPosition === null || hoverTime === null) return null;

  const formatTime = (time: number) => {
    const minutes = Math.floor(time / 60);
    const seconds = Math.floor(time % 60);
    return `${minutes}:${seconds.toString().padStart(2, "0")}`;
  };

  return (
    <div
      ref={ref}
      className={cn(
        "pointer-events-none absolute top-0 flex flex-col items-center",
        className,
      )}
      style={{ left: hoverPosition }}
      {...props}
    >
      {/* Vertical line */}
      <div className="bg-foreground/30 h-full w-[1px]" />
      {/* Timestamp tooltip */}
      <div className="bg-background border-border text-foreground absolute -top-8 rounded border px-2 py-1 text-xs shadow-md">
        {formatTime(hoverTime)}
      </div>
    </div>
  );
});
AudioVisualizerHover.displayName = "AudioVisualizerHover";

// Time display component - can work both with and without context
interface AudioVisualizerTimeProps {
  format?: "mm:ss" | "seconds";
  showDuration?: boolean;
  className?: string;
  currentTime?: number;
  duration?: number;
}

const AudioVisualizerTime = forwardRef<
  HTMLDivElement,
  AudioVisualizerTimeProps
>(
  (
    {
      format = "mm:ss",
      showDuration = true,
      className,
      currentTime: propCurrentTime,
      duration: propDuration,
      ...props
    },
    ref,
  ) => {
    // Safely try to get context, but don't throw if not available
    const context = useAudioVisualizerSafe();

    // Use props if provided, otherwise fall back to context values
    const currentTime = propCurrentTime ?? context?.currentTime ?? 0;
    const duration = propDuration ?? context?.duration ?? 0;

    const formatTime = (time: number) => {
      if (format === "seconds") {
        return `${time.toFixed(1)}s`;
      }
      const minutes = Math.floor(time / 60);
      const seconds = Math.floor(time % 60);
      return `${minutes}:${seconds.toString().padStart(2, "0")}`;
    };

    return (
      <div
        ref={ref}
        className={cn("text-muted-foreground font-mono text-sm", className)}
        {...props}
      >
        {formatTime(currentTime)}
        {showDuration && duration > 0 && <span> / {formatTime(duration)}</span>}
      </div>
    );
  },
);
AudioVisualizerTime.displayName = "AudioVisualizerTime";

export {
  AudioVisualizerCanvas,
  AudioVisualizerHover,
  AudioVisualizerProgress,
  AudioVisualizerRoot,
  AudioVisualizerTime,
};
