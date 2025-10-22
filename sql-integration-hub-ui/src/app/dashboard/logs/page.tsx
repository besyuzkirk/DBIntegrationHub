'use client';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Loader2, CheckCircle2, XCircle, Clock, Database } from 'lucide-react';
import { useLogs } from '@/hooks/useLogs';
import { formatDistanceToNow } from 'date-fns';
import { tr } from 'date-fns/locale';

export default function LogsPage() {
  const { data: logs, isLoading, error } = useLogs(100);

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
            Loglar yüklenirken bir hata oluştu.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Integration Logs</h1>
        <p className="text-muted-foreground">
          Tüm integration çalıştırma geçmişini görüntüleyin
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Son Loglar</CardTitle>
          <CardDescription>
            {logs?.length || 0} log kaydı
          </CardDescription>
        </CardHeader>
        <CardContent>
          {logs?.length === 0 ? (
            <div className="text-center py-12">
              <p className="text-muted-foreground">
                Henüz log kaydı yok.
              </p>
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Durum</TableHead>
                  <TableHead>Çalıştırma Zamanı</TableHead>
                  <TableHead>Satır Sayısı</TableHead>
                  <TableHead>Süre</TableHead>
                  <TableHead>Mesaj</TableHead>
                  <TableHead>Hata</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {logs?.map((log) => (
                  <TableRow key={log.id}>
                    <TableCell>
                      {log.success ? (
                        <Badge variant="default" className="bg-green-500">
                          <CheckCircle2 className="mr-1 h-3 w-3" />
                          Başarılı
                        </Badge>
                      ) : (
                        <Badge variant="destructive">
                          <XCircle className="mr-1 h-3 w-3" />
                          Başarısız
                        </Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-sm">
                      {formatDistanceToNow(new Date(log.runDate), {
                        addSuffix: true,
                        locale: tr,
                      })}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Database className="h-4 w-4 text-muted-foreground" />
                        <span className="font-medium">{log.rowCount.toLocaleString()}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Clock className="h-4 w-4 text-muted-foreground" />
                        <span className="text-sm">
                          {log.durationMs < 1000
                            ? `${log.durationMs}ms`
                            : `${(log.durationMs / 1000).toFixed(2)}s`}
                        </span>
                      </div>
                    </TableCell>
                    <TableCell className="max-w-xs truncate text-sm">
                      {log.message || '-'}
                    </TableCell>
                    <TableCell className="max-w-xs truncate text-sm text-red-500">
                      {log.errorDetails || '-'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
