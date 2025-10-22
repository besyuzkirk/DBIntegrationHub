import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';

export interface IntegrationLog {
  id: string;
  integrationId: string;
  runDate: string;
  success: boolean;
  message?: string;
  rowCount: number;
  durationMs: number;
  errorDetails?: string;
}

export function useLogs(limit: number = 100) {
  return useQuery({
    queryKey: ['logs', limit],
    queryFn: async () => {
      const { data } = await apiClient.get<IntegrationLog[]>(`/logs?limit=${limit}`);
      return data;
    },
  });
}

export function useLogsByIntegration(integrationId: string | undefined) {
  return useQuery({
    queryKey: ['logs', 'integration', integrationId],
    queryFn: async () => {
      if (!integrationId) return null;
      const { data } = await apiClient.get<IntegrationLog[]>(`/logs/integration/${integrationId}`);
      return data;
    },
    enabled: !!integrationId,
  });
}
