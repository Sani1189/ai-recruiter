"use client";

import { useEffect, useState } from "react";
import { useApi } from "./useApi";

export interface CountryListItem {
  countryCode: string;
  name: string;
}

export function useCountries() {
  const { get } = useApi();
  const [countries, setCountries] = useState<CountryListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError(null);
    get<CountryListItem[]>("/country")
      .then((res) => {
        if (cancelled) return;
        const data = res?.data ?? res;
        setCountries(Array.isArray(data) ? data : []);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof Error ? err : new Error(String(err)));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [get]);

  return { countries, loading, error };
}
