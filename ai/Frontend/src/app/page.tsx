"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import Header from "@/components/layout/Header";
import Hero from "@/components/pages/_candidate/home/Hero";

import { useAuthStore } from "@/stores/useAuthStore";

export default function Home() {
  const router = useRouter();
  const { userType } = useAuthStore();

  useEffect(() => {
    if (!userType) return;

    const redirectMap = {
      candidate: "/profile",
      recruiter: "/recruiter/dashboard",
    };

    router.push(redirectMap[userType] || "/");
  }, [userType]);

  return (
    <>
      <Header variant="candidate" />
      <main className="from-primary/10 via-background to-secondary/5 bg-gradient-to-br">
        <Hero />
        {/* <Features />
        <Stats />
        <CTA /> */}
      </main>
    </>
  );
}
