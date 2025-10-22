import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';
import { toast } from 'sonner';

export interface Mapping {
  id: string;
  integrationId: string;
  sourceColumn: string;
  targetParameter: string;
}

export interface SaveMappingsRequest {
  integrationId: string;
  mappings: { sourceColumn: string; targetParameter: string }[];
}

export function useMappings(integrationId: string | undefined) {
  return useQuery({
    queryKey: ['mappings', integrationId],
    queryFn: async () => {
      if (!integrationId) return null;
      const { data } = await apiClient.get<Mapping[]>(`/mappings/${integrationId}`);
      return data;
    },
    enabled: !!integrationId,
  });
}

export function useSaveMappings() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: SaveMappingsRequest) => {
      const { data } = await apiClient.post<{ success: boolean }>('/mappings', request);
      return data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['mappings', variables.integrationId] });
      toast.success('Mapping başarıyla kaydedildi');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Mapping kaydedilirken hata oluştu');
    },
  });
}
