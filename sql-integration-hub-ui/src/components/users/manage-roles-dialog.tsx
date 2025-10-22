'use client';

import { useState } from 'react';
import { useRoles, useAssignRole, useRemoveRole } from '@/hooks/useUsers';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Loader2, Plus, X } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

interface ManageRolesDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  userId: string;
  currentRoles: string[];
}

export function ManageRolesDialog({
  open,
  onOpenChange,
  userId,
  currentRoles,
}: ManageRolesDialogProps) {
  const [selectedRole, setSelectedRole] = useState<string>('');
  
  const { data: allRoles, isLoading: rolesLoading } = useRoles();
  const assignMutation = useAssignRole();
  const removeMutation = useRemoveRole();

  const availableRoles = allRoles?.filter((role) => !currentRoles.includes(role.name)) || [];

  const handleAssignRole = () => {
    if (selectedRole) {
      assignMutation.mutate(
        { userId, roleName: selectedRole },
        {
          onSuccess: () => {
            setSelectedRole('');
          },
        }
      );
    }
  };

  const handleRemoveRole = (roleName: string) => {
    removeMutation.mutate({ userId, roleName });
  };

  const isLoading = assignMutation.isPending || removeMutation.isPending;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Rolleri Yönet</DialogTitle>
          <DialogDescription>
            Kullanıcının rollerini ekleyin veya çıkarın.
          </DialogDescription>
        </DialogHeader>
        
        <div className="space-y-6 py-4">
          {/* Current Roles */}
          <div className="space-y-2">
            <label className="text-sm font-medium">Mevcut Roller</label>
            <div className="flex flex-wrap gap-2">
              {currentRoles.length > 0 ? (
                currentRoles.map((role) => (
                  <Badge
                    key={role}
                    variant="secondary"
                    className="flex items-center gap-2 px-3 py-1"
                  >
                    {role}
                    <button
                      onClick={() => handleRemoveRole(role)}
                      disabled={isLoading || role === 'Admin'}
                      className="hover:text-destructive disabled:opacity-50 disabled:cursor-not-allowed"
                      title={role === 'Admin' ? 'Admin rolü çıkarılamaz' : 'Rolü çıkar'}
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))
              ) : (
                <p className="text-sm text-muted-foreground">Henüz rol yok</p>
              )}
            </div>
          </div>

          {/* Add New Role */}
          {availableRoles.length > 0 && (
            <div className="space-y-2">
              <label className="text-sm font-medium">Yeni Rol Ekle</label>
              <div className="flex gap-2">
                <Select value={selectedRole} onValueChange={setSelectedRole} disabled={isLoading}>
                  <SelectTrigger className="flex-1">
                    <SelectValue placeholder="Rol seçin" />
                  </SelectTrigger>
                  <SelectContent>
                    {rolesLoading ? (
                      <div className="flex items-center justify-center p-2">
                        <Loader2 className="h-4 w-4 animate-spin" />
                      </div>
                    ) : (
                      availableRoles.map((role) => (
                        <SelectItem key={role.id} value={role.name}>
                          {role.name}
                          {role.description && (
                            <span className="text-muted-foreground ml-2">
                              - {role.description}
                            </span>
                          )}
                        </SelectItem>
                      ))
                    )}
                  </SelectContent>
                </Select>
                <Button
                  onClick={handleAssignRole}
                  disabled={!selectedRole || isLoading}
                  size="icon"
                >
                  {isLoading ? (
                    <Loader2 className="h-4 w-4 animate-spin" />
                  ) : (
                    <Plus className="h-4 w-4" />
                  )}
                </Button>
              </div>
            </div>
          )}

          {availableRoles.length === 0 && currentRoles.length > 0 && (
            <p className="text-sm text-muted-foreground">
              Bu kullanıcı tüm rollere sahip.
            </p>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

