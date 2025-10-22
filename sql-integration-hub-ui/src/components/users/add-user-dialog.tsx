'use client';

import { useState } from 'react';
import { useCreateUser, useRoles } from '@/hooks/useUsers';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Loader2 } from 'lucide-react';

interface AddUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function AddUserDialog({ open, onOpenChange }: AddUserDialogProps) {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);

  const { data: roles, isLoading: rolesLoading } = useRoles();
  const createMutation = useCreateUser();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    createMutation.mutate(
      {
        username,
        email,
        password,
        roleNames: selectedRoles,
      },
      {
        onSuccess: () => {
          onOpenChange(false);
          // Reset form
          setUsername('');
          setEmail('');
          setPassword('');
          setSelectedRoles([]);
        },
      }
    );
  };

  const toggleRole = (roleName: string) => {
    setSelectedRoles((prev) =>
      prev.includes(roleName)
        ? prev.filter((r) => r !== roleName)
        : [...prev, roleName]
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Yeni Kullanıcı Ekle</DialogTitle>
            <DialogDescription>
              Sisteme yeni kullanıcı ekleyin ve rollerini atayın.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="username">Kullanıcı Adı *</Label>
              <Input
                id="username"
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="kullaniciadi"
                required
                disabled={createMutation.isPending}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="email">Email *</Label>
              <Input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="email@example.com"
                required
                disabled={createMutation.isPending}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Şifre *</Label>
              <Input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="En az 6 karakter"
                required
                minLength={6}
                disabled={createMutation.isPending}
              />
            </div>
            <div className="space-y-2">
              <Label>Roller *</Label>
              {rolesLoading ? (
                <div className="flex items-center gap-2">
                  <Loader2 className="h-4 w-4 animate-spin" />
                  <span className="text-sm text-muted-foreground">Roller yükleniyor...</span>
                </div>
              ) : (
                <div className="space-y-2">
                  {roles?.map((role) => (
                    <div key={role.id} className="flex items-center space-x-2">
                      <Checkbox
                        id={`role-${role.id}`}
                        checked={selectedRoles.includes(role.name)}
                        onCheckedChange={() => toggleRole(role.name)}
                        disabled={createMutation.isPending}
                      />
                      <Label
                        htmlFor={`role-${role.id}`}
                        className="text-sm font-normal cursor-pointer"
                      >
                        {role.name}
                        {role.description && (
                          <span className="text-muted-foreground ml-2">
                            - {role.description}
                          </span>
                        )}
                      </Label>
                    </div>
                  ))}
                </div>
              )}
              {selectedRoles.length === 0 && (
                <p className="text-xs text-amber-600">En az bir rol seçmelisiniz</p>
              )}
            </div>
          </div>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={createMutation.isPending}
            >
              İptal
            </Button>
            <Button
              type="submit"
              disabled={createMutation.isPending || selectedRoles.length === 0}
            >
              {createMutation.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Oluşturuluyor...
                </>
              ) : (
                'Kullanıcı Oluştur'
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

