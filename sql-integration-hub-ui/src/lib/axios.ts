import axios from 'axios';

// API base URL - ortam değişkeninden al veya default kullan
const baseURL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5149/api';

// Store'u temizlemek için global bir fonksiyon
let clearAuthStore: (() => void) | null = null;

export const setClearAuthStore = (fn: () => void) => {
  clearAuthStore = fn;
};

// Axios instance oluştur
export const apiClient = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 saniye
});

// Request interceptor - token ekleme
apiClient.interceptors.request.use(
  (config) => {
    // Token'ı localStorage'dan al ve header'a ekle
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor - hata yönetimi
apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  (error) => {
    // 401 Unauthorized - Token geçersiz veya süresi dolmuş
    if (error.response?.status === 401) {
      // Token'ı temizle
      localStorage.removeItem('token');
      
      // Store'u temizle (sadece browser'daysa)
      if (typeof window !== 'undefined') {
        // Store'u temizle
        if (clearAuthStore) {
          clearAuthStore();
        }
        // Login sayfasına yönlendir
        window.location.href = '/login';
      }
    }
    
    // Hata durumlarını loglama
    if (error.response) {
      // Sunucu yanıt verdi ama hata kodu döndü
      console.error('API Error:', error.response.data);
    } else if (error.request) {
      // İstek yapıldı ama yanıt alınamadı
      console.error('Network Error:', error.request);
    } else {
      // İstek oluşturulurken hata oluştu
      console.error('Error:', error.message);
    }
    return Promise.reject(error);
  }
);

export default apiClient;

