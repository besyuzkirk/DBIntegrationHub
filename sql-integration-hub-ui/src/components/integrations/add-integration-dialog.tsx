'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Plus, Eye, Loader2 } from 'lucide-react';
import { useConnections } from '@/hooks/useConnections';
import { useCreateIntegration, usePreviewQuery, QueryPreviewResult } from '@/hooks/useIntegrations';
import { VisualQueryBuilder } from './VisualQueryBuilder';

// Connection tipini tanımla
interface Connection {
  id: string;
  name: string;
  databaseType: string;
}

interface FormData {
  name: string;
  sourceConnectionId: string;
  targetConnectionId: string;
  sourceQuery: string;
  targetQuery: string;
  groupName?: string;
  executionOrder: number;
}

export function AddIntegrationDialog() {
  const [open, setOpen] = useState(false);
  const [previewData, setPreviewData] = useState<QueryPreviewResult | null>(null);
  const { data: connections, isLoading: connectionsLoading } = useConnections();
  const createIntegration = useCreateIntegration();
  const previewMutation = usePreviewQuery();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch,
  } = useForm<FormData>({
    defaultValues: {
      name: '',
      sourceConnectionId: '',
      targetConnectionId: '',
      sourceQuery: '',
      targetQuery: '',
      groupName: '',
      executionOrder: 0,
    },
  });

  const sourceConnectionId = watch('sourceConnectionId');
  const targetConnectionId = watch('targetConnectionId');
  const sourceQuery = watch('sourceQuery');

  const handlePreviewSource = async () => {
    if (!sourceConnectionId || !sourceQuery) return;

    try {
      const result = await previewMutation.mutateAsync({
        connectionId: sourceConnectionId,
        query: sourceQuery,
      });
      setPreviewData(result);
    } catch (error) {
      // Error handled by mutation hook
    }
  };

  const onSubmit = async (data: FormData) => {
    try {
      await createIntegration.mutateAsync(data);
      reset();
      setPreviewData(null);
      setOpen(false);
    } catch (error) {
      // Error handling yapıldı (toast ile)
    }
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="mr-2 h-4 w-4" />
          Yeni Integration
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Yeni Integration Oluştur</DialogTitle>
          <DialogDescription>
            Kaynak ve hedef veritabanları arasında bir integration tanımlayın.
          </DialogDescription>
        </DialogHeader>

        <Tabs defaultValue="form" className="w-full">
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="form">Form</TabsTrigger>
            <TabsTrigger value="visual">Visual Builder</TabsTrigger>
            <TabsTrigger value="preview">Preview</TabsTrigger>
          </TabsList>

          <TabsContent value="form" className="mt-4">
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              {/* Name */}
              <div className="space-y-2">
                <Label htmlFor="name">Integration Adı</Label>
                <Input
                  id="name"
                  placeholder="Örn: User Data Sync"
                  {...register('name', {
                    required: 'Integration adı gereklidir',
                  })}
                />
                {errors.name && (
                  <p className="text-sm text-red-500">{errors.name.message}</p>
                )}
              </div>

              {/* Source Connection */}
              <div className="space-y-2">
                <Label htmlFor="sourceConnectionId">Kaynak Bağlantı</Label>
                <Select
                  value={sourceConnectionId}
                  onValueChange={(value: string) => setValue('sourceConnectionId', value)}
                >
                  <SelectTrigger id="sourceConnectionId">
                    <SelectValue placeholder="Kaynak bağlantı seçin" />
                  </SelectTrigger>
                  <SelectContent>
                    {connectionsLoading ? (
                      <SelectItem value="loading" disabled>
                        Yükleniyor...
                      </SelectItem>
                    ) : connections?.length === 0 ? (
                      <SelectItem value="empty" disabled>
                        Bağlantı bulunamadı
                      </SelectItem>
                    ) : (
                      connections?.map((conn: Connection) => (
                        <SelectItem key={conn.id} value={conn.id}>
                          {conn.name} ({conn.databaseType})
                        </SelectItem>
                      ))
                    )}
                  </SelectContent>
                </Select>
                {errors.sourceConnectionId && (
                  <p className="text-sm text-red-500">
                    {errors.sourceConnectionId.message}
                  </p>
                )}
              </div>

              {/* Source Query */}
              <div className="space-y-2">
                <Label htmlFor="sourceQuery">Kaynak Sorgu (SELECT)</Label>
                <Textarea
                  id="sourceQuery"
                  placeholder='SELECT "Id", "Name", "Email" FROM "Users"'
                  rows={4}
                  className="font-mono text-sm"
                  {...register('sourceQuery', {
                    required: 'Kaynak sorgu gereklidir',
                    pattern: {
                      value: /^\s*SELECT/i,
                      message: 'Sorgu SELECT ile başlamalıdır',
                    },
                  })}
                />
                {errors.sourceQuery && (
                  <p className="text-sm text-red-500">
                    {errors.sourceQuery.message}
                  </p>
                )}
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={handlePreviewSource}
                  disabled={!sourceConnectionId || !sourceQuery || previewMutation.isPending}
                  className="w-full"
                >
                  {previewMutation.isPending ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Yükleniyor...
                    </>
                  ) : (
                    <>
                      <Eye className="mr-2 h-4 w-4" />
                      Preview Source
                    </>
                  )}
                </Button>
              </div>

              {/* Target Connection */}
              <div className="space-y-2">
                <Label htmlFor="targetConnectionId">Hedef Bağlantı</Label>
                <Select
                  value={targetConnectionId}
                  onValueChange={(value: string) => setValue('targetConnectionId', value)}
                >
                  <SelectTrigger id="targetConnectionId">
                    <SelectValue placeholder="Hedef bağlantı seçin" />
                  </SelectTrigger>
                  <SelectContent>
                    {connectionsLoading ? (
                      <SelectItem value="loading" disabled>
                        Yükleniyor...
                      </SelectItem>
                    ) : connections?.length === 0 ? (
                      <SelectItem value="empty" disabled>
                        Bağlantı bulunamadı
                      </SelectItem>
                    ) : (
                      connections?.map((conn: Connection) => (
                        <SelectItem key={conn.id} value={conn.id}>
                          {conn.name} ({conn.databaseType})
                        </SelectItem>
                      ))
                    )}
                  </SelectContent>
                </Select>
                {errors.targetConnectionId && (
                  <p className="text-sm text-red-500">
                    {errors.targetConnectionId.message}
                  </p>
                )}
              </div>

              {/* Target Query */}
              <div className="space-y-2">
                <Label htmlFor="targetQuery">Hedef Sorgu (INSERT/UPDATE)</Label>
                <Textarea
                  id="targetQuery"
                  placeholder="INSERT INTO target_users (id, name, email) VALUES (@Id, @Name, @Email)"
                  rows={4}
                  className="font-mono text-sm"
                  {...register('targetQuery', {
                    required: 'Hedef sorgu gereklidir',
                    pattern: {
                      value: /\b(INSERT|UPDATE)\b/i,
                      message: 'Sorgu INSERT veya UPDATE içermelidir',
                    },
                  })}
                />
                {errors.targetQuery && (
                  <p className="text-sm text-red-500">
                    {errors.targetQuery.message}
                  </p>
                )}
              </div>

              {/* Group Name (Optional) */}
              <div className="space-y-2">
                <Label htmlFor="groupName">Grup Adı (Opsiyonel)</Label>
                <Input
                  id="groupName"
                  placeholder="Örn: Daily Sync"
                  {...register('groupName')}
                />
                <p className="text-xs text-muted-foreground">
                  Aynı gruptaki integration'lar toplu çalıştırılabilir
                </p>
              </div>

              {/* Execution Order */}
              <div className="space-y-2">
                <Label htmlFor="executionOrder">Çalışma Sırası</Label>
                <Input
                  id="executionOrder"
                  type="number"
                  {...register('executionOrder', { valueAsNumber: true })}
                />
                <p className="text-xs text-muted-foreground">
                  Grup içinde hangi sırada çalışacağını belirler (küçükten büyüğe)
                </p>
              </div>

              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    reset();
                    setPreviewData(null);
                    setOpen(false);
                  }}
                >
                  İptal
                </Button>
                <Button type="submit" disabled={createIntegration.isPending}>
                  {createIntegration.isPending ? 'Oluşturuluyor...' : 'Oluştur'}
                </Button>
              </DialogFooter>
            </form>
          </TabsContent>

          <TabsContent value="visual" className="mt-4">
            <div className="space-y-6">
              {/* Basic Info */}
              <Card>
                <CardHeader>
                  <CardTitle>Integration Bilgileri</CardTitle>
                  <CardDescription>
                    Temel integration bilgilerini girin
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Name */}
                  <div className="space-y-2">
                    <Label htmlFor="visual-name">Integration Adı</Label>
                    <Input
                      id="visual-name"
                      placeholder="Örn: User Data Sync"
                      {...register('name', {
                        required: 'Integration adı gereklidir',
                      })}
                    />
                    {errors.name && (
                      <p className="text-sm text-red-500">{errors.name.message}</p>
                    )}
                  </div>

                  {/* Source Connection */}
                  <div className="space-y-2">
                    <Label htmlFor="visual-sourceConnectionId">Kaynak Bağlantı</Label>
                    <Select
                      value={sourceConnectionId}
                      onValueChange={(value: string) => setValue('sourceConnectionId', value)}
                    >
                      <SelectTrigger id="visual-sourceConnectionId">
                        <SelectValue placeholder="Kaynak bağlantı seçin" />
                      </SelectTrigger>
                      <SelectContent>
                        {connectionsLoading ? (
                          <SelectItem value="loading" disabled>
                            Yükleniyor...
                          </SelectItem>
                        ) : connections?.length === 0 ? (
                          <SelectItem value="empty" disabled>
                            Bağlantı bulunamadı
                          </SelectItem>
                        ) : (
                          connections?.map((conn: any) => (
                            <SelectItem key={conn.id} value={conn.id}>
                              {conn.name} ({conn.databaseType})
                            </SelectItem>
                          ))
                        )}
                      </SelectContent>
                    </Select>
                    {errors.sourceConnectionId && (
                      <p className="text-sm text-red-500">
                        {errors.sourceConnectionId.message}
                      </p>
                    )}
                  </div>

                  {/* Target Connection */}
                  <div className="space-y-2">
                    <Label htmlFor="visual-targetConnectionId">Hedef Bağlantı</Label>
                    <Select
                      value={targetConnectionId}
                      onValueChange={(value: string) => setValue('targetConnectionId', value)}
                    >
                      <SelectTrigger id="visual-targetConnectionId">
                        <SelectValue placeholder="Hedef bağlantı seçin" />
                      </SelectTrigger>
                      <SelectContent>
                        {connectionsLoading ? (
                          <SelectItem value="loading" disabled>
                            Yükleniyor...
                          </SelectItem>
                        ) : connections?.length === 0 ? (
                          <SelectItem value="empty" disabled>
                            Bağlantı bulunamadı
                          </SelectItem>
                        ) : (
                          connections?.map((conn: any) => (
                            <SelectItem key={conn.id} value={conn.id}>
                              {conn.name} ({conn.databaseType})
                            </SelectItem>
                          ))
                        )}
                      </SelectContent>
                    </Select>
                    {errors.targetConnectionId && (
                      <p className="text-sm text-red-500">
                        {errors.targetConnectionId.message}
                      </p>
                    )}
                  </div>

                  {/* Group Name and Execution Order */}
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <Label htmlFor="visual-groupName">Grup Adı (Opsiyonel)</Label>
                      <Input
                        id="visual-groupName"
                        placeholder="Örn: Daily Sync"
                        {...register('groupName')}
                      />
                    </div>
                    <div className="space-y-2">
                      <Label htmlFor="visual-executionOrder">Çalışma Sırası</Label>
                      <Input
                        id="visual-executionOrder"
                        type="number"
                        {...register('executionOrder', { valueAsNumber: true })}
                      />
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Visual Query Builder */}
              <VisualQueryBuilder
                connectionId={sourceConnectionId}
                onQueryChange={(query) => setValue('sourceQuery', query)}
                initialQuery={sourceQuery}
              />

              {/* Target Query */}
              <Card>
                <CardHeader>
                  <CardTitle>Hedef Sorgu (INSERT/UPDATE)</CardTitle>
                  <CardDescription>
                    Kaynak verilerin hedef veritabanına nasıl yazılacağını belirleyin
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <Textarea
                    placeholder="INSERT INTO target_users (id, name, email) VALUES (@Id, @Name, @Email)"
                    rows={4}
                    className="font-mono text-sm"
                    {...register('targetQuery', {
                      required: 'Hedef sorgu gereklidir',
                      pattern: {
                        value: /\b(INSERT|UPDATE)\b/i,
                        message: 'Sorgu INSERT veya UPDATE içermelidir',
                      },
                    })}
                  />
                  {errors.targetQuery && (
                    <p className="text-sm text-red-500 mt-2">
                      {errors.targetQuery.message}
                    </p>
                  )}
                </CardContent>
              </Card>

              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    reset();
                    setPreviewData(null);
                    setOpen(false);
                  }}
                >
                  İptal
                </Button>
                <Button 
                  onClick={handleSubmit(onSubmit)} 
                  disabled={createIntegration.isPending}
                >
                  {createIntegration.isPending ? 'Oluşturuluyor...' : 'Oluştur'}
                </Button>
              </DialogFooter>
            </div>
          </TabsContent>

          <TabsContent value="preview" className="mt-4">
            {previewData && previewData.rows.length > 0 ? (
              <div className="space-y-4">
                <div className="text-sm text-muted-foreground">
                  {previewData.rowCount} satır döndü (ilk 100 satır gösteriliyor)
                </div>
                <div className="border rounded-lg overflow-auto max-h-[400px]">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        {previewData.columns.map((col) => (
                          <TableHead key={col} className="font-semibold">
                            {col}
                          </TableHead>
                        ))}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {previewData.rows.map((row, idx) => (
                        <TableRow key={idx}>
                          {previewData.columns.map((col) => (
                            <TableCell key={col} className="font-mono text-xs">
                              {row[col] !== null && row[col] !== undefined
                                ? String(row[col])
                                : <span className="text-muted-foreground">null</span>}
                            </TableCell>
                          ))}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            ) : (
              <div className="text-center py-12 text-muted-foreground">
                <Eye className="w-12 h-12 mx-auto mb-4 opacity-20" />
                <p>Henüz preview verisi yok.</p>
                <p className="text-sm">
                  Form sekmesinde "Preview Source" butonuna tıklayın.
                </p>
              </div>
            )}
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  );
}
