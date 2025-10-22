import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';

// Types
export interface DashboardStatistics {
  totalConnections: number;
  activeConnections: number;
  inactiveConnections: number;
  totalIntegrations: number;
  totalMappings: number;
  totalLogs: number;
  todayLogs: number;
  successfulLogsToday: number;
  failedLogsToday: number;
  successRateToday: number;
  overallSuccessRate: number;
}

export interface RecentIntegrationLog {
  id: string;
  integrationId: string;
  integrationName: string;
  runDate: string;
  success: boolean;
  rowCount: number;
  durationMs: number;
  message?: string;
  errorDetails?: string;
}

export interface IntegrationActivity {
  date: string;
  totalRuns: number;
  successfulRuns: number;
  failedRuns: number;
  successRate: number;
}

// API Functions
const getDashboardStatistics = async (): Promise<DashboardStatistics> => {
  const response = await apiClient.get('/dashboard/statistics');
  return response.data;
};

const getRecentLogs = async (count: number = 10): Promise<RecentIntegrationLog[]> => {
  const response = await apiClient.get(`/dashboard/recent-logs?count=${count}`);
  return response.data;
};

const getIntegrationActivity = async (days: number = 7): Promise<IntegrationActivity[]> => {
  const response = await apiClient.get(`/dashboard/activity?days=${days}`);
  return response.data;
};

// Hooks
export const useDashboardStatistics = () => {
  return useQuery({
    queryKey: ['dashboard', 'statistics'],
    queryFn: getDashboardStatistics,
    refetchInterval: 30000, // 30 saniyede bir yenile
  });
};

export const useRecentLogs = (count: number = 10) => {
  return useQuery({
    queryKey: ['dashboard', 'recent-logs', count],
    queryFn: () => getRecentLogs(count),
    refetchInterval: 30000,
  });
};

export const useIntegrationActivity = (days: number = 7) => {
  return useQuery({
    queryKey: ['dashboard', 'activity', days],
    queryFn: () => getIntegrationActivity(days),
    refetchInterval: 60000, // 1 dakikada bir yenile
  });
};

