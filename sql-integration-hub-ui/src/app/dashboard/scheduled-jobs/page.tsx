'use client';

import { useState } from 'react';
import { useScheduledJobs } from '@/hooks/useScheduledJobs';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Clock,
  Plus,
  Play,
  Pause,
  Trash2,
  CheckCircle2,
  XCircle,
  Activity,
} from 'lucide-react';
import { CreateScheduledJobDialog } from '@/components/scheduled-jobs/create-scheduled-job-dialog';
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
import { formatDistanceToNow } from 'date-fns';
import { tr } from 'date-fns/locale';
import { useStore } from '@/store/useStore';

export default function ScheduledJobsPage() {
  const { hasRole } = useStore();
  const {
    scheduledJobs,
    isLoading,
    deleteScheduledJob,
    toggleScheduledJob,
    triggerScheduledJob,
  } = useScheduledJobs();

  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [jobToDelete, setJobToDelete] = useState<string | null>(null);

  if (!hasRole('Admin')) {
    return (
      <div className="p-8">
        <Card>
          <CardContent className="pt-6">
            <p className="text-center text-muted-foreground">
              Bu sayfaya erişim izniniz yok. Sadece yöneticiler zamanlanmış işleri
              görebilir.
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const handleDelete = async () => {
    if (jobToDelete) {
      await deleteScheduledJob(jobToDelete);
      setJobToDelete(null);
    }
  };

  const handleToggle = async (id: string) => {
    await toggleScheduledJob(id);
  };

  const handleTrigger = async (id: string) => {
    await triggerScheduledJob(id);
  };

  if (isLoading) {
    return (
      <div className="p-8">
        <div className="flex items-center justify-center">
          <p>Yükleniyor...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Zamanlanmış İşler</h1>
          <p className="text-muted-foreground mt-2">
            Integration'larınızı otomatik olarak belirli zamanlarda çalıştırın
          </p>
        </div>
        <Button onClick={() => setIsCreateDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Yeni İş Ekle
        </Button>
      </div>

      {/* İstatistikler */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Toplam İş</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{scheduledJobs?.length || 0}</div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Aktif İş</CardTitle>
            <Activity className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {scheduledJobs?.filter((j) => j.isActive).length || 0}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Başarılı</CardTitle>
            <CheckCircle2 className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {scheduledJobs?.reduce((sum, j) => sum + j.successfulRuns, 0) || 0}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Başarısız</CardTitle>
            <XCircle className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {scheduledJobs?.reduce((sum, j) => sum + j.failedRuns, 0) || 0}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tablo */}
      <Card>
        <CardHeader>
          <CardTitle>Zamanlanmış İşler</CardTitle>
        </CardHeader>
        <CardContent>
          {!scheduledJobs || scheduledJobs.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              Henüz zamanlanmış iş yok. Yeni bir iş eklemek için yukarıdaki butona
              tıklayın.
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>İsim</TableHead>
                    <TableHead>Integration</TableHead>
                    <TableHead>Cron</TableHead>
                    <TableHead>Durum</TableHead>
                    <TableHead>İstatistikler</TableHead>
                    <TableHead>Son Çalışma</TableHead>
                    <TableHead>Sonraki Çalışma</TableHead>
                    <TableHead>İşlemler</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {scheduledJobs.map((job) => (
                    <TableRow key={job.id}>
                      <TableCell>
                        <div>
                          <div className="font-medium">{job.name}</div>
                          <div className="text-sm text-muted-foreground">
                            {job.description}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        {job.integrationName || (
                          <span className="text-muted-foreground italic">
                            Grup
                          </span>
                        )}
                      </TableCell>
                      <TableCell>
                        <code className="text-xs bg-muted px-2 py-1 rounded">
                          {job.cronExpression}
                        </code>
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={job.isActive ? 'default' : 'secondary'}
                        >
                          {job.isActive ? 'Aktif' : 'Pasif'}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="text-sm space-y-1">
                          <div className="flex items-center gap-2">
                            <CheckCircle2 className="h-3 w-3 text-green-600" />
                            <span>{job.successfulRuns}</span>
                          </div>
                          <div className="flex items-center gap-2">
                            <XCircle className="h-3 w-3 text-red-600" />
                            <span>{job.failedRuns}</span>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        {job.lastRunAt
                          ? formatDistanceToNow(new Date(job.lastRunAt), {
                              addSuffix: true,
                              locale: tr,
                            })
                          : '-'}
                      </TableCell>
                      <TableCell>
                        {job.nextRunAt
                          ? formatDistanceToNow(new Date(job.nextRunAt), {
                              addSuffix: true,
                              locale: tr,
                            })
                          : '-'}
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => handleTrigger(job.id)}
                            title="Şimdi Çalıştır"
                          >
                            <Play className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => handleToggle(job.id)}
                            title={
                              job.isActive
                                ? 'Pasif Yap'
                                : 'Aktif Yap'
                            }
                          >
                            {job.isActive ? (
                              <Pause className="h-4 w-4" />
                            ) : (
                              <Activity className="h-4 w-4" />
                            )}
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => setJobToDelete(job.id)}
                            title="Sil"
                          >
                            <Trash2 className="h-4 w-4 text-red-600" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      <CreateScheduledJobDialog
        open={isCreateDialogOpen}
        onOpenChange={setIsCreateDialogOpen}
      />

      <AlertDialog open={!!jobToDelete} onOpenChange={() => setJobToDelete(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Emin misiniz?</AlertDialogTitle>
            <AlertDialogDescription>
              Bu zamanlanmış işi silmek istediğinize emin misiniz? Bu işlem geri
              alınamaz.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>İptal</AlertDialogCancel>
            <AlertDialogAction onClick={handleDelete}>Sil</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

