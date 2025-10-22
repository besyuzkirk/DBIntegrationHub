import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';
import { toast } from 'react-toastify';
import { AxiosError } from 'axios';

export interface Connection {
  id: string;
  name: string;
  databaseType: 'PostgreSQL' | 'MySQL' | 'SQLServer' | 'MongoDB';
  connectionString: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateConnectionRequest {
  name: string;
  databaseType: 'PostgreSQL' | 'MySQL' | 'SQLServer' | 'MongoDB';
  connectionString: string;
}

export interface TestConnectionRequest {
  connectionString: string;
  databaseType: 'PostgreSQL' | 'MySQL' | 'SQLServer' | 'MongoDB';
}

export interface TestConnectionResponse {
  isSuccess: boolean;
  message: string;
  responseTimeMs?: number;
}

// GET all connections
export function useConnections() {
  return useQuery({
    queryKey: ['connections'],
    queryFn: async () => {
      const response = await apiClient.get<Connection[]>('/connections');
      return response.data;
    },
  });
}

// POST create connection
export function useCreateConnection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateConnectionRequest) => {
      const response = await apiClient.post<{ id: string }>('/connections', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['connections'] });
      toast.success('Connection created successfully!');
    },
    onError: (error: Error | AxiosError) => {
      const message = error instanceof AxiosError 
        ? error.response?.data?.error || error.message 
        : error.message;
      toast.error('Failed to create connection: ' + message);
    },
  });
}

// POST test connection
export function useTestConnection() {
  return useMutation({
    mutationFn: async (data: TestConnectionRequest) => {
      const response = await apiClient.post<TestConnectionResponse>('/connections/test', data);
      return response.data;
    },
    onSuccess: (data) => {
      if (data.isSuccess) {
        toast.success(`Connection test successful! (${data.responseTimeMs}ms)`);
      } else {
        toast.error(data.message);
      }
    },
    onError: (error: Error | AxiosError) => {
      const message = error instanceof AxiosError 
        ? error.response?.data?.error || error.message 
        : error.message;
      toast.error('Connection test failed: ' + message);
    },
  });
}

// DELETE connection
export function useDeleteConnection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/connections/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['connections'] });
      toast.success('Connection deleted successfully!');
    },
    onError: (error: Error | AxiosError) => {
      const message = error instanceof AxiosError 
        ? error.response?.data?.error || error.message 
        : error.message;
      toast.error('Failed to delete connection: ' + message);
    },
  });
}

