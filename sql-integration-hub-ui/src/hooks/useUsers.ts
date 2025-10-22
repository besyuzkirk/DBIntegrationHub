import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';
import { toast } from 'sonner';

export interface User {
  id: string;
  username: string;
  email: string;
  isActive: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  roles: string[];
}

export interface Role {
  id: string;
  name: string;
  description: string;
}

interface CreateUserRequest {
  username: string;
  email: string;
  password: string;
  roleNames: string[];
}

// Get all users
export function useUsers() {
  return useQuery({
    queryKey: ['users'],
    queryFn: async () => {
      const response = await apiClient.get<User[]>('/users');
      return response.data;
    },
  });
}

// Get all roles
export function useRoles() {
  return useQuery({
    queryKey: ['roles'],
    queryFn: async () => {
      const response = await apiClient.get<Role[]>('/users/roles');
      return response.data;
    },
  });
}

// Create user
export function useCreateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (data: CreateUserRequest) => {
      const response = await apiClient.post('/users', data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('Kullanıcı başarıyla oluşturuldu');
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Kullanıcı oluşturulurken bir hata oluştu';
      toast.error(message);
    },
  });
}

// Delete user
export function useDeleteUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (userId: string) => {
      await apiClient.delete(`/users/${userId}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('Kullanıcı başarıyla silindi');
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Kullanıcı silinirken bir hata oluştu';
      toast.error(message);
    },
  });
}

// Toggle user status
export function useToggleUserStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (userId: string) => {
      await apiClient.patch(`/users/${userId}/toggle-status`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('Kullanıcı durumu güncellendi');
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Durum güncellenirken bir hata oluştu';
      toast.error(message);
    },
  });
}

// Assign role
export function useAssignRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ userId, roleName }: { userId: string; roleName: string }) => {
      await apiClient.post(`/users/${userId}/roles`, { roleName });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('Rol başarıyla atandı');
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Rol atanırken bir hata oluştu';
      toast.error(message);
    },
  });
}

// Remove role
export function useRemoveRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ userId, roleName }: { userId: string; roleName: string }) => {
      await apiClient.delete(`/users/${userId}/roles/${roleName}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] });
      toast.success('Rol başarıyla çıkarıldı');
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Rol çıkarılırken bir hata oluştu';
      toast.error(message);
    },
  });
}

