'use client';

import { useState } from 'react';
import { useUsers, useDeleteUser, useToggleUserStatus } from '@/hooks/useUsers';
import { useAuth } from '@/hooks/useAuth';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Users, UserPlus, Trash2, Power, Shield } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { tr } from 'date-fns/locale';
import { AddUserDialog } from '@/components/users/add-user-dialog';
import { ManageRolesDialog } from '@/components/users/manage-roles-dialog';

export default function UsersPage() {
  const { hasRole } = useAuth();
  const { data: users, isLoading } = useUsers();
  const deleteMutation = useDeleteUser();
  const toggleStatusMutation = useToggleUserStatus();

  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [userToDelete, setUserToDelete] = useState<string | null>(null);
  const [addUserDialogOpen, setAddUserDialogOpen] = useState(false);
  const [manageRolesDialogOpen, setManageRolesDialogOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);

  // Admin değilse erişimi engelle
  if (!hasRole('Admin')) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <Shield className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
          <h2 className="text-xl font-semibold mb-2">Yetki Gerekli</h2>
          <p className="text-muted-foreground">
            Bu sayfaya erişmek için Admin yetkisine sahip olmanız gerekiyor.
          </p>
        </div>
      </div>
    );
  }

  const handleDeleteClick = (userId: string) => {
    setUserToDelete(userId);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = () => {
    if (userToDelete) {
      deleteMutation.mutate(userToDelete);
      setDeleteDialogOpen(false);
      setUserToDelete(null);
    }
  };

  const handleToggleStatus = (userId: string) => {
    toggleStatusMutation.mutate(userId);
  };

  const handleManageRoles = (userId: string) => {
    setSelectedUserId(userId);
    setManageRolesDialogOpen(true);
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-4xl font-light text-foreground mb-2">
            Kullanıcı Yönetimi
          </h1>
          <p className="text-muted-foreground font-light">
            Sistem kullanıcılarını yönetin
          </p>
        </div>
        <Button onClick={() => setAddUserDialogOpen(true)} className="gap-2">
          <UserPlus className="h-4 w-4" />
          Yeni Kullanıcı
        </Button>
      </div>

      {/* Users Table */}
      <Card className="zen-card">
        <CardHeader>
          <CardTitle className="text-2xl font-light flex items-center gap-2">
            <Users className="h-6 w-6" />
            Kullanıcılar
          </CardTitle>
          <CardDescription className="font-light">
            {users?.length || 0} kullanıcı
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : users && users.length > 0 ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Kullanıcı</TableHead>
                    <TableHead>Email</TableHead>
                    <TableHead>Roller</TableHead>
                    <TableHead>Son Giriş</TableHead>
                    <TableHead>Durum</TableHead>
                    <TableHead className="text-right">İşlemler</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {users.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell className="font-medium">{user.username}</TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>
                        <div className="flex gap-1 flex-wrap">
                          {user.roles.map((role) => (
                            <Badge key={role} variant="secondary" className="text-xs">
                              {role}
                            </Badge>
                          ))}
                        </div>
                      </TableCell>
                      <TableCell className="text-sm text-muted-foreground">
                        {user.lastLoginAt
                          ? formatDistanceToNow(new Date(user.lastLoginAt), {
                              addSuffix: true,
                              locale: tr,
                            })
                          : 'Hiç giriş yapmadı'}
                      </TableCell>
                      <TableCell>
                        <Badge variant={user.isActive ? 'default' : 'secondary'}>
                          {user.isActive ? 'Aktif' : 'Pasif'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex gap-2 justify-end">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleManageRoles(user.id)}
                          >
                            <Shield className="h-4 w-4 mr-1" />
                            Roller
                          </Button>
                          {user.username !== 'admin' && (
                            <>
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => handleToggleStatus(user.id)}
                              >
                                <Power className="h-4 w-4 mr-1" />
                                {user.isActive ? 'Pasifleştir' : 'Aktifleştir'}
                              </Button>
                              <Button
                                variant="destructive"
                                size="sm"
                                onClick={() => handleDeleteClick(user.id)}
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            </>
                          )}
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <Users className="w-12 h-12 mx-auto mb-2 opacity-50" />
              <p>Henüz kullanıcı yok</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Add User Dialog */}
      <AddUserDialog open={addUserDialogOpen} onOpenChange={setAddUserDialogOpen} />

      {/* Manage Roles Dialog */}
      {selectedUserId && (
        <ManageRolesDialog
          open={manageRolesDialogOpen}
          onOpenChange={setManageRolesDialogOpen}
          userId={selectedUserId}
          currentRoles={users?.find((u) => u.id === selectedUserId)?.roles || []}
        />
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Kullanıcıyı Sil</AlertDialogTitle>
            <AlertDialogDescription>
              Bu kullanıcıyı silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>İptal</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteConfirm} className="bg-destructive text-destructive-foreground">
              Sil
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

