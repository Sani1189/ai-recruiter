'use client';

import { useState } from 'react';
import { useApi } from '@/hooks/useApi';
import { jobsService } from '@/lib/api/services/jobs.service';

export function SimpleApiExample() {
  const api = useApi();
  const [data, setData] = useState<any>(null);

  // Method 1: Use the hook directly (automatic token injection)
  const fetchWithHook = async () => {
    try {
      const response = await api.get('/recruiter/jobs');
      setData(response.data);
    } catch (error) {
      console.error('Error:', error);
    }
  };

  // Method 2: Use existing services (they still work as before)
  const fetchWithService = async () => {
    try {
      const response = await jobsService.getJobs();
      setData(response.data);
    } catch (error) {
      console.error('Error:', error);
    }
  };

  // Method 3: Public API (no token needed)
  const checkHealth = async () => {
    try {
      const response = await api.get('/health', { requireAuth: false });
      setData(response.data);
    } catch (error) {
      console.error('Error:', error);
    }
  };

  return (
    <div>
      <h2>Simple API Example</h2>
      <button onClick={fetchWithHook}>Fetch with Hook (Auto Token)</button>
      <button onClick={fetchWithService}>Fetch with Service (Manual Token)</button>
      <button onClick={checkHealth}>Health Check (No Token)</button>
      
      {data && (
        <pre>{JSON.stringify(data, null, 2)}</pre>
      )}
    </div>
  );
}
