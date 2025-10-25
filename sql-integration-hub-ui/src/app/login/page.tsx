'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useRouter } from 'next/navigation';
import { Database, Loader2 } from 'lucide-react';

export default function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const { login, isLoggingIn, isAuthenticated } = useAuth();
  const router = useRouter();

  // Eğer zaten giriş yapmışsa dashboard'a yönlendir
  useEffect(() => {
    // Sadece gerçekten authenticate ise yönlendir
    if (isAuthenticated && !isLoggingIn) {
      router.push('/dashboard');
    }
  }, [isAuthenticated, isLoggingIn, router]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login({ username, password });
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="flex items-center justify-center mb-4">
            <div className="p-3 bg-primary/10 rounded-full">
              <Database className="h-10 w-10 text-primary" />
            </div>
          </div>
          <CardTitle className="text-2xl font-bold">DB Integration Hub</CardTitle>
          <CardDescription>
            Veritabanı entegrasyon yönetim sistemi
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="username">Kullanıcı Adı</Label>
              <Input
                id="username"
                type="text"
                placeholder="admin"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                disabled={isLoggingIn}
                required
                autoFocus
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Şifre</Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={isLoggingIn}
                required
              />
            </div>
            <Button 
              type="submit" 
              className="w-full" 
              disabled={isLoggingIn || !username || !password}
            >
              {isLoggingIn ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Giriş yapılıyor...
                </>
              ) : (
                'Giriş Yap'
              )}
            </Button>
          </form>

          <div className="mt-6 space-y-3">
            <div className="text-center text-sm text-muted-foreground">
              <p>Varsayılan kullanıcı:</p>
              <p className="font-mono text-xs mt-1">
                Kullanıcı: admin | Şifre: admin
              </p>
            </div>
            <div className="bg-amber-50 dark:bg-amber-950 border border-amber-200 dark:border-amber-800 rounded-lg p-3">
              <p className="text-xs text-amber-800 dark:text-amber-200 font-medium text-center">
                ⚠️ Güvenlik Uyarısı
              </p>
              <p className="text-xs text-amber-700 dark:text-amber-300 mt-1 text-center">
                İlk girişten sonra şifrenizi mutlaka değiştirin!<br />
                Varsayılan şifre güvenli değildir.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

