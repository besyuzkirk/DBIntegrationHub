import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useStore, User } from '@/store/useStore';
import { apiClient } from '@/lib/axios';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';

interface LoginRequest {
  username: string;
  password: string;
}

interface LoginResponse {
  token: string;
  username: string;
  email: string;
  roles: string[];
}

interface ChangePasswordRequest {
  username: string;
  currentPassword: string;
  newPassword: string;
}

export function useAuth() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { user, token, isAuthenticated, setAuth, setUser, logout: logoutStore, hasRole } = useStore();

  // Login mutation
  const loginMutation = useMutation({
    mutationFn: async (credentials: LoginRequest) => {
      const response = await apiClient.post<LoginResponse>('/auth/login', credentials);
      return response.data;
    },
    onSuccess: async (data) => {
      // Token'ı localStorage'a kaydet
      localStorage.setItem('token', data.token);
      
      // Önce token ile kullanıcı bilgilerini al
      try {
        const userResponse = await apiClient.get<User>('/auth/me', {
          headers: { Authorization: `Bearer ${data.token}` }
        });
        
        // Store'u gerçek user data ile güncelle
        setAuth(userResponse.data, data.token);
      } catch (error) {
        // Eğer /me çağrısı başarısız olursa, login response'tan user oluştur
        const user: User = {
          id: '',
          username: data.username,
          email: data.email,
          isActive: true,
          lastLoginAt: new Date().toISOString(),
          roles: data.roles,
        };
        setAuth(user, data.token);
      }
      
      // Dashboard'a yönlendir
      router.push('/dashboard');
      toast.success('Giriş başarılı!');
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Giriş yapılırken bir hata oluştu';
      toast.error(message);
    },
  });

  // Logout
  const logout = () => {
    logoutStore();
    queryClient.clear();
    router.push('/login');
    toast.success('Çıkış yapıldı');
  };

  // Get current user
  const { data: currentUser, isLoading: isLoadingUser } = useQuery({
    queryKey: ['currentUser'],
    queryFn: async () => {
      const response = await apiClient.get<User>('/auth/me');
      return response.data;
    },
    enabled: isAuthenticated && !!token,
    retry: false,
    staleTime: 5 * 60 * 1000, // 5 dakika
  });

  // Update user in store when fetched
  if (currentUser && currentUser.id !== user?.id) {
    setUser(currentUser);
  }

  // Change password mutation
  const changePasswordMutation = useMutation({
    mutationFn: async (data: ChangePasswordRequest) => {
      await apiClient.post('/auth/change-password', data);
    },
    onSuccess: () => {
      toast.success('Şifre başarıyla değiştirildi');
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Şifre değiştirirken bir hata oluştu';
      toast.error(message);
    },
  });

  return {
    user,
    token,
    isAuthenticated,
    isLoadingUser,
    login: loginMutation.mutate,
    isLoggingIn: loginMutation.isPending,
    logout,
    changePassword: changePasswordMutation.mutate,
    isChangingPassword: changePasswordMutation.isPending,
    hasRole,
  };
}

