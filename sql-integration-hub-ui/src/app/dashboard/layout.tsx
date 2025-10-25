'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useStore } from '@/store/useStore';
import { Sidebar } from '@/components/layout/sidebar';
import { Navbar } from '@/components/layout/navbar';
import { Toaster } from 'sonner';
import { Loader2 } from 'lucide-react';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const { isAuthenticated, token } = useStore();

  useEffect(() => {
    // Eğer authenticate değilse login sayfasına yönlendir
    // Kısa bir gecikme ile kontrol et (store rehydration için)
    const checkAuth = () => {
      if (!isAuthenticated || !token) {
        router.push('/login');
      }
    };

    const timeoutId = setTimeout(checkAuth, 100);
    
    return () => clearTimeout(timeoutId);
  }, [isAuthenticated, token, router]);

  // Auth kontrolü yapılırken loading göster
  if (!isAuthenticated || !token) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Sidebar - Desktop */}
      <Sidebar />

      {/* Main content */}
      <div className="lg:pl-64">
        {/* Navbar */}
        <Navbar />

        {/* Page content */}
        <main className="py-8">
          <div className="zen-container">
            {children}
          </div>
        </main>
      </div>

      {/* Toast notifications */}
      <Toaster richColors position="top-right" />
    </div>
  );
}

