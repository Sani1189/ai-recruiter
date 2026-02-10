import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { Toaster } from "@/components/ui/sonner";
import { UnifiedAuthProvider } from "@/lib/UnifiedAuthProvider";

import "@/styles/globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Recruiter Sign In - Osilion Recruitment",
  description: "Sign in for recruiters and hiring managers",
};

export default function RecruiterSignInLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
        <UnifiedAuthProvider>
          {children}
          <Toaster />
        </UnifiedAuthProvider>
  );
}
