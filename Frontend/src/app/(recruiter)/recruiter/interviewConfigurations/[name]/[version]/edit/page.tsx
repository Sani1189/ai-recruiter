"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { useApi } from "@/hooks/useApi";
import { InterviewConfiguration } from "@/types/interviewConfiguration";
import InterviewConfigurationForm from "@/components/pages/_recruiter/recruiter/interviewConfigurations/InterviewConfigurationForm";

export default function EditInterviewConfigurationPage() {
  const params = useParams();
  const api = useApi();
  const [configuration, setConfiguration] = useState<InterviewConfiguration | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const name = params.name as string;
  const version = parseInt(params.version as string);

  useEffect(() => {
    if (!name || !version) return;

    const fetchConfiguration = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await api.get(`/InterviewConfiguration/${name}/${version}`);
        
        // Standard approach - use response.data if available, otherwise response
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
  }, [name, version]); // Only depend on name and version

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

  return <InterviewConfigurationForm configuration={configuration} mode="edit" />;
}
