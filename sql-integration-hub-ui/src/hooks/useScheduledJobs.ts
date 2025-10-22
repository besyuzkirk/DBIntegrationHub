import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';
import { toast } from 'sonner';

export interface ScheduledJob {
  id: string;
  name: string;
  description: string;
  cronExpression: string;
  isActive: boolean;
  integrationId?: string;
  integrationName?: string;
  groupId?: string;
  lastRunAt?: string;
  nextRunAt?: string;
  totalRuns: number;
  successfulRuns: number;
  failedRuns: number;
  createdAt: string;
}

export interface CreateScheduledJobDto {
  name: string;
  description: string;
  cronExpression: string;
  integrationId?: string;
  groupId?: string;
}

export function useScheduledJobs() {
  const queryClient = useQueryClient();

  // Tüm scheduled jobs'ları getir
  const {
    data: scheduledJobs,
    isLoading,
    error,
  } = useQuery<ScheduledJob[]>({
    queryKey: ['scheduled-jobs'],
    queryFn: async () => {
      const response = await apiClient.get('/api/scheduled-jobs');
      return response.data;
    },
  });

  // Yeni scheduled job oluştur
  const createMutation = useMutation({
    mutationFn: async (data: CreateScheduledJobDto) => {
      const response = await apiClient.post('/api/scheduled-jobs', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] });
      toast.success('Zamanlanmış iş başarıyla oluşturuldu');
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.error || 'Zamanlanmış iş oluşturulamadı');
    },
  });

  // Scheduled job sil
  const deleteMutation = useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/scheduled-jobs/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] });
      toast.success('Zamanlanmış iş silindi');
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.error || 'Zamanlanmış iş silinemedi');
    },
  });

  // Scheduled job aktif/pasif yap
  const toggleMutation = useMutation({
    mutationFn: async (id: string) => {
      await apiClient.patch(`/api/scheduled-jobs/${id}/toggle`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] });
      toast.success('Zamanlanmış iş durumu güncellendi');
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.error || 'Zamanlanmış iş durumu güncellenemedi');
    },
  });

  // Scheduled job şimdi çalıştır
  const triggerMutation = useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/api/scheduled-jobs/${id}/trigger`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['scheduled-jobs'] });
      toast.success('Zamanlanmış iş tetiklendi');
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.error || 'Zamanlanmış iş tetiklenemedi');
    },
  });

  return {
    scheduledJobs,
    isLoading,
    error,
    createScheduledJob: createMutation.mutateAsync,
    deleteScheduledJob: deleteMutation.mutateAsync,
    toggleScheduledJob: toggleMutation.mutateAsync,
    triggerScheduledJob: triggerMutation.mutateAsync,
  };
}

