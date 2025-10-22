'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
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
import { Badge } from '@/components/ui/badge';
import { Trash2, Loader2, Eye, Clock } from 'lucide-react';
import { AddIntegrationDialog } from '@/components/integrations/add-integration-dialog';
import { CreateScheduledJobDialog } from '@/components/scheduled-jobs/create-scheduled-job-dialog';
import { useIntegrations, useDeleteIntegration } from '@/hooks/useIntegrations';
import { formatDistanceToNow } from 'date-fns';
import { tr } from 'date-fns/locale';
import { useStore } from '@/store/useStore';

export default function IntegrationsPage() {
  const { data: integrations, isLoading, error } = useIntegrations();
  const deleteIntegration = useDeleteIntegration();
  const { hasRole } = useStore();
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [scheduleIntegrationId, setScheduleIntegrationId] = useState<
    string | null
  >(null);

  const handleDelete = async () => {
    if (deleteId) {
      await deleteIntegration.mutateAsync(deleteId);
      setDeleteId(null);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <p className="text-lg font-semibold text-red-500">Hata Oluştu</p>
          <p className="text-sm text-muted-foreground mt-2">
            Integration'lar yüklenirken bir hata oluştu.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Integrations</h1>
          <p className="text-muted-foreground">
            Kaynak ve hedef veritabanları arasındaki integration'ları yönetin.
          </p>
        </div>
        <AddIntegrationDialog />
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Tüm Integration'lar</CardTitle>
          <CardDescription>
            {integrations?.length || 0} integration tanımlı
          </CardDescription>
        </CardHeader>
        <CardContent>
          {integrations?.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-muted-foreground">
                Henüz integration tanımlanmamış.
              </p>
              <p className="text-sm text-muted-foreground mt-2">
                Yeni bir integration oluşturmak için yukarıdaki butona tıklayın.
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Ad</TableHead>
                  <TableHead>Kaynak Bağlantı</TableHead>
                  <TableHead>Hedef Bağlantı</TableHead>
                  <TableHead>Kaynak Sorgu</TableHead>
                  <TableHead>Hedef Sorgu</TableHead>
                  <TableHead>Oluşturulma</TableHead>
                  <TableHead className="text-right">İşlemler</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {integrations?.map((integration) => (
                  <TableRow key={integration.id}>
                    <TableCell className="font-medium">
                      {integration.name}
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline">
                        {integration.sourceConnectionName}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <Badge variant="outline">
                        {integration.targetConnectionName}
                      </Badge>
                    </TableCell>
                    <TableCell className="max-w-xs">
                      <code className="text-xs bg-muted px-2 py-1 rounded block truncate">
                        {integration.sourceQuery}
                      </code>
                    </TableCell>
                    <TableCell className="max-w-xs">
                      <code className="text-xs bg-muted px-2 py-1 rounded block truncate">
                        {integration.targetQuery}
                      </code>
                    </TableCell>
                    <TableCell className="text-sm text-muted-foreground">
                      {formatDistanceToNow(new Date(integration.createdAt), {
                        addSuffix: true,
                        locale: tr,
                      })}
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex items-center justify-end gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => {
                            // TODO: View details modal
                            console.log('View integration:', integration.id);
                          }}
                          title="Detayları Görüntüle"
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        {hasRole('Admin') && (
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() =>
                              setScheduleIntegrationId(integration.id)
                            }
                            title="Zamanla"
                          >
                            <Clock className="h-4 w-4 text-blue-500" />
                          </Button>
                        )}
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDeleteId(integration.id)}
                          title="Sil"
                        >
                          <Trash2 className="h-4 w-4 text-red-500" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Emin misiniz?</AlertDialogTitle>
            <AlertDialogDescription>
              Bu integration kalıcı olarak silinecek. Bu işlem geri alınamaz.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>İptal</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="bg-red-500 hover:bg-red-600"
            >
              Sil
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Schedule Integration Dialog */}
      <CreateScheduledJobDialog
        open={!!scheduleIntegrationId}
        onOpenChange={(open) => {
          if (!open) setScheduleIntegrationId(null);
        }}
        preSelectedIntegrationId={scheduleIntegrationId || undefined}
      />
    </div>
  );
}
