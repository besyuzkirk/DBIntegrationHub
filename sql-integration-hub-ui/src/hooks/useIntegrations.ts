import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';
import { toast } from 'sonner';

// Types
export interface Integration {
  id: string;
  name: string;
  sourceConnectionId: string;
  sourceConnectionName: string;
  targetConnectionId: string;
  targetConnectionName: string;
  sourceQuery: string;
  targetQuery: string;
  groupName?: string;
  executionOrder: number;
  createdAt: string;
}

export interface CreateIntegrationRequest {
  name: string;
  sourceConnectionId: string;
  targetConnectionId: string;
  sourceQuery: string;
  targetQuery: string;
  groupName?: string;
  executionOrder?: number;
}

export interface IntegrationColumns {
  sourceColumns: string[];
  targetParameters: string[];
}

export interface RunIntegrationResponse {
  success: boolean;
  rowsAffected: number;
  durationMs: number;
  message?: string;
  error?: string;
}

// Hooks
export function useIntegrations() {
  return useQuery({
    queryKey: ['integrations'],
    queryFn: async () => {
      const { data } = await apiClient.get<Integration[]>('/integrations');
      return data;
    },
  });
}

export function useIntegration(id: string | undefined) {
  return useQuery({
    queryKey: ['integrations', id],
    queryFn: async () => {
      if (!id) return null;
      const { data } = await apiClient.get<Integration>(`/integrations/${id}`);
      return data;
    },
    enabled: !!id,
  });
}

export function useIntegrationColumns(id: string | undefined) {
  return useQuery({
    queryKey: ['integrations', id, 'columns'],
    queryFn: async () => {
      if (!id) return null;
      const { data } = await apiClient.get<IntegrationColumns>(
        `/integrations/${id}/columns`
      );
      return data;
    },
    enabled: !!id,
  });
}

export function useCreateIntegration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: CreateIntegrationRequest) => {
      const { data } = await apiClient.post<{ id: string }>('/integrations', request);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['integrations'] });
      toast.success('Integration başarıyla oluşturuldu');
    },
    onError: (error: any) => {
      toast.error(
        error.response?.data?.error || 'Integration oluşturulurken hata oluştu'
      );
    },
  });
}

export function useDeleteIntegration() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/integrations/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['integrations'] });
      toast.success('Integration başarıyla silindi');
    },
    onError: (error: any) => {
      toast.error(
        error.response?.data?.error || 'Integration silinirken hata oluştu'
      );
    },
  });
}

export function useRunIntegration() {
  return useMutation({
    mutationFn: async (id: string) => {
      const { data } = await apiClient.post<RunIntegrationResponse>(
        `/integrations/${id}/run`
      );
      return data;
    },
    onError: (error: any) => {
      toast.error(
        error.response?.data?.error || 'Integration çalıştırılırken hata oluştu'
      );
    },
  });
}

export interface BatchRunResult {
  success: boolean;
  totalRowsAffected: number;
  totalDurationMs: number;
  results: Array<{
    integrationId: string;
    integrationName: string;
    success: boolean;
    rowsAffected: number;
    error?: string;
  }>;
  error?: string;
}

export function useBatchRunIntegrations() {
  return useMutation({
    mutationFn: async (integrationIds: string[]) => {
      const { data } = await apiClient.post<BatchRunResult>(
        `/integrations/batch-run`,
        { integrationIds }
      );
      return data;
    },
    onSuccess: (data) => {
      if (data.success) {
        toast.success(`Batch run başarılı! ${data.totalRowsAffected} satır etkilendi.`);
      } else {
        toast.error(data.error || 'Batch run başarısız oldu');
      }
    },
    onError: (error: any) => {
      toast.error(
        error.response?.data?.error || 'Batch run sırasında hata oluştu'
      );
    },
  });
}

export interface QueryPreviewRequest {
  connectionId: string;
  query: string;
}

export interface QueryPreviewResult {
  columns: string[];
  rows: Array<Record<string, any>>;
  rowCount: number;
}

export function usePreviewQuery() {
  return useMutation({
    mutationFn: async (request: QueryPreviewRequest) => {
      const { data } = await apiClient.post<QueryPreviewResult>(
        `/connections/preview-query`,
        request
      );
      return data;
    },
    onError: (error: any) => {
      toast.error(
        error.response?.data?.error || 'Query preview sırasında hata oluştu'
      );
    },
  });
}
