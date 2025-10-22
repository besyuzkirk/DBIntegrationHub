'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode, useState } from 'react';

export function QueryProvider({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            // Veriler 5 dakika sonra stale olur
            staleTime: 5 * 60 * 1000,
            // Cache 10 dakika saklanır
            gcTime: 10 * 60 * 1000,
            // Pencere odaklandığında otomatik refetch
            refetchOnWindowFocus: false,
            // Mount olduğunda refetch
            refetchOnMount: true,
            // Yeniden deneme sayısı
            retry: 1,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}

