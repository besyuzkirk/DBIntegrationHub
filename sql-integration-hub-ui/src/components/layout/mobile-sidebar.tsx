'use client';

import Link from 'next/link';
import Image from 'next/image';
import { usePathname } from 'next/navigation';
import { cn } from '@/lib/utils';
import {
  LayoutDashboard,
  Database,
  GitBranch,
  ArrowLeftRight,
  FileText,
  Play,
  Users,
  Clock,
} from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';

const navigation = [
  {
    name: 'Dashboard',
    href: '/dashboard',
    icon: LayoutDashboard,
  },
  {
    name: 'Connections',
    href: '/dashboard/connections',
    icon: Database,
  },
  {
    name: 'Integrations',
    href: '/dashboard/integrations',
    icon: GitBranch,
  },
  {
    name: 'Run',
    href: '/dashboard/integrations/run',
    icon: Play,
  },
  {
    name: 'Mappings',
    href: '/dashboard/mappings',
    icon: ArrowLeftRight,
  },
  {
    name: 'Logs',
    href: '/dashboard/logs',
    icon: FileText,
  },
  {
    name: 'Users',
    href: '/dashboard/users',
    icon: Users,
    adminOnly: true, // Sadece Admin görebilir
  },
  {
    name: 'Scheduled Jobs',
    href: '/dashboard/scheduled-jobs',
    icon: Clock,
    adminOnly: true, // Sadece Admin görebilir
  },
];

export function MobileSidebar() {
  const pathname = usePathname();
  const { hasRole } = useAuth();

  return (
    <div className="flex flex-col h-full bg-card">
      {/* Logo */}
      <div className="flex items-center h-16 px-6 border-b border-border">
        <h1 className="text-xl font-semibold text-foreground">
          DB Integration Hub
        </h1>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-4 py-6 space-y-1 overflow-y-auto">
        {navigation.filter(item => !item.adminOnly || hasRole('Admin')).map((item) => {
          let isActive = false;
          
          if (item.href === '/dashboard') {
            // Dashboard sadece tam eşleşmede aktif olsun
            isActive = pathname === '/dashboard';
          } else if (item.href === '/dashboard/integrations') {
            // Integrations için Run sayfası hariç
            isActive = (pathname === '/dashboard/integrations' || 
                       (pathname?.startsWith('/dashboard/integrations/') && 
                        pathname !== '/dashboard/integrations/run'));
          } else if (item.href === '/dashboard/integrations/run') {
            // Run için sadece exact match veya alt sayfalar
            isActive = pathname === '/dashboard/integrations/run' || 
                       pathname?.startsWith('/dashboard/integrations/run/');
          } else {
            // Diğer sayfalar için normal kontrol
            isActive = pathname === item.href || pathname?.startsWith(item.href + '/');
          }
          
          return (
            <Link
              key={item.name}
              href={item.href}
              className={cn(
                'flex items-center gap-3 px-4 py-3 text-sm font-light rounded-xl transition-all duration-300',
                isActive
                  ? 'bg-primary text-primary-foreground shadow-sm'
                  : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
              )}
            >
              <item.icon className="w-5 h-5" />
              <span>{item.name}</span>
            </Link>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="p-4 border-t border-border">
        <div className="flex items-center justify-center gap-2">
          <p className="text-xs text-muted-foreground font-light">
            by FawnStudios
          </p>
          <Image 
            src="/fawnlogo.png" 
            alt="FawnStudios Logo" 
            width={16} 
            height={16}
            className="object-contain"
            priority
          />
        </div>
      </div>
    </div>
  );
}

