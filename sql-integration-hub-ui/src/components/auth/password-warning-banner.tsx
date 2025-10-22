'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { AlertTriangle, X } from 'lucide-react';
import { ChangePasswordDialog } from './change-password-dialog';

export function PasswordWarningBanner() {
  const { user } = useAuth();
  const [isVisible, setIsVisible] = useState(false);
  const [isDismissed, setIsDismissed] = useState(false);

  useEffect(() => {
    // Admin kullanıcısı ve daha önce dismiss edilmemişse göster
    if (user?.username === 'admin' && !isDismissed) {
      const dismissed = localStorage.getItem('password-warning-dismissed');
      if (!dismissed) {
        setIsVisible(true);
      } else {
        setIsDismissed(true);
      }
    }
  }, [user, isDismissed]);

  const handleDismiss = () => {
    localStorage.setItem('password-warning-dismissed', 'true');
    setIsDismissed(true);
    setIsVisible(false);
  };

  if (!isVisible || !user) return null;

  return (
    <Alert className="mb-6 border-amber-200 bg-amber-50 dark:border-amber-800 dark:bg-amber-950">
      <AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400" />
      <AlertTitle className="text-amber-800 dark:text-amber-200">
        Varsayılan Şifre Kullanımda!
      </AlertTitle>
      <AlertDescription className="text-amber-700 dark:text-amber-300">
        <div className="flex items-start justify-between gap-4">
          <div className="flex-1">
            <p className="mb-3">
              Güvenliğiniz için lütfen varsayılan şifreyi değiştirin. 
              Varsayılan şifreler güvenli değildir ve yetkisiz erişime açıktır.
            </p>
            <div className="flex gap-2">
              <ChangePasswordDialog>
                <Button size="sm" variant="default" className="bg-amber-600 hover:bg-amber-700 text-white">
                  Şimdi Değiştir
                </Button>
              </ChangePasswordDialog>
              <Button size="sm" variant="outline" onClick={handleDismiss}>
                Daha Sonra
              </Button>
            </div>
          </div>
          <Button
            size="icon"
            variant="ghost"
            onClick={handleDismiss}
            className="h-6 w-6 text-amber-600 hover:text-amber-700"
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      </AlertDescription>
    </Alert>
  );
}

