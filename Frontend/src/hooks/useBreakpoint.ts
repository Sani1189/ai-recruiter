import { useEffect, useState } from "react";

type Breakpoint = "none" | "xs" | "sm" | "md" | "lg" | "xl" | "2xl";

const breakpoints = {
  none: 0,
  xs: 448,
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  "2xl": 1536,
} as const;

export function useBreakpoint(): Breakpoint {
  const [breakpoint, setBreakpoint] = useState<Breakpoint>("xs");

  useEffect(() => {
    const updateBreakpoint = () => {
      const width = window.innerWidth;

      if (width >= breakpoints["2xl"]) {
        setBreakpoint("2xl");
      } else if (width >= breakpoints.xl) {
        setBreakpoint("xl");
      } else if (width >= breakpoints.lg) {
        setBreakpoint("lg");
      } else if (width >= breakpoints.md) {
        setBreakpoint("md");
      } else if (width >= breakpoints.sm) {
        setBreakpoint("sm");
      } else if (width >= breakpoints.xs) {
        setBreakpoint("xs");
      } else {
        setBreakpoint("none");
      }
    };

    // Set initial breakpoint
    updateBreakpoint();

    // Add event listener
    window.addEventListener("resize", updateBreakpoint);

    // Cleanup
    return () => {
      window.removeEventListener("resize", updateBreakpoint);
    };
  }, []);

  return breakpoint;
}

// Optional: Export utility functions for checking specific breakpoints
export function useIsBreakpoint(targetBreakpoint: Breakpoint): boolean {
  const currentBreakpoint = useBreakpoint();
  const breakpointKeys = Object.keys(breakpoints) as Breakpoint[];
  const currentIndex = breakpointKeys.indexOf(currentBreakpoint);
  const targetIndex = breakpointKeys.indexOf(targetBreakpoint);

  return currentIndex >= targetIndex;
}
