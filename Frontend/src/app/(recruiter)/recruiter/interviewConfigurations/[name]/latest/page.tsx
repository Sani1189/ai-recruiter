"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { useApi } from "@/hooks/useApi";
import { InterviewConfiguration } from "@/types/interviewConfiguration";
import InterviewConfigurationView from "@/components/pages/_recruiter/recruiter/interviewConfigurations/InterviewConfigurationView";

export default function LatestInterviewConfigurationPage() {
  const params = useParams();
  const api = useApi();
  const [configuration, setConfiguration] = useState<InterviewConfiguration | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const name = params.name as string;

  
  useEffect(() => {
    if (!name) return;    
    const fetchConfiguration = async () => {
      try {
        setLoading(true);
        setError(null);
        console.log('Fetching latest interview configuration for:', name);
        const response = await api.get(`/InterviewConfiguration/${name}/latest`);
        console.log('API Response:', response);
        
        const configData = response.data || response;
        setConfiguration(configData);
      } catch (err) {
        console.error('Failed to fetch interview configuration:', err);
        setError('Failed to load interview configuration');
      } finally {
        setLoading(false);
      }
    };

    fetchConfiguration();
  }, [name]);

  if (loading) {
    return (
      <div className="container py-8">
        <div className="flex items-center justify-center py-8">
          <div className="text-muted-foreground">Loading interview configuration...</div>
        </div>
      </div>
    );
  }

  if (error || !configuration) {
    return (
      <div className="container py-8">
        <div className="flex items-center justify-center py-8">
          <div className="text-destructive">{error || 'Interview configuration not found'}</div>
        </div>
      </div>
    );
  }

  return <InterviewConfigurationView configuration={configuration} />;
}

