"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";

import { useApi } from "@/hooks/useApi";
import { Prompt } from "@/types/prompt";
import PromptForm from "@/components/pages/_recruiter/recruiter/prompts/PromptForm";

export default function EditPromptPage() {
  const params = useParams();
  const api = useApi();
  
  const [prompt, setPrompt] = useState<Prompt | null>(null);
  const [loading, setLoading] = useState(true);

  const name = params.name as string;
  const version = parseInt(params.version as string);

  useEffect(() => {
    const fetchPrompt = async () => {
      try {
        const response = await api.get(`/prompt/${name}/${version}`);
        console.log('Edit prompt response:', response);
        
        // Handle both ApiResponse wrapper and direct data
        const data = response.data || response;
        if (data && data.name) {
          setPrompt(data);
        }
      } catch (error) {
        console.error('Failed to fetch prompt:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchPrompt();
  }, [name, version]);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (!prompt) {
    return <div>Prompt not found</div>;
  }

  return <PromptForm prompt={prompt} mode="edit" />;
}
