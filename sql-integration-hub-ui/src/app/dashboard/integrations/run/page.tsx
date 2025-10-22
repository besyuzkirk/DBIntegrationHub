'use client';

import { useState, useEffect, Suspense } from 'react';
import { useSearchParams } from 'next/navigation';
import { useIntegrations, useRunIntegration, useBatchRunIntegrations, RunIntegrationResponse, BatchRunResult } from '@/hooks/useIntegrations';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Loader2, Play, CheckCircle2, XCircle, Clock, Database, Activity, Users } from 'lucide-react';
import { toast } from 'sonner';

// Dynamic rendering için force
export const dynamic = 'force-dynamic';

type RunMode = 'single' | 'batch' | 'group';

function RunIntegrationContent() {
  const searchParams = useSearchParams();
  const integrationParam = searchParams.get('integration');
  
  const [runMode, setRunMode] = useState<RunMode>('single');
  const [selectedIntegrationId, setSelectedIntegrationId] = useState<string | null>(null);
  const [selectedGroup, setSelectedGroup] = useState<string | null>(null);
  const [selectedIntegrationIds, setSelectedIntegrationIds] = useState<string[]>([]);
  const [runResult, setRunResult] = useState<RunIntegrationResponse | null>(null);
  const [batchResult, setBatchResult] = useState<BatchRunResult | null>(null);

  const { data: integrations, isLoading: isLoadingIntegrations } = useIntegrations();
  const runMutation = useRunIntegration();
  const batchRunMutation = useBatchRunIntegrations();

  // Auto-select integration from URL param
  useEffect(() => {
    if (integrationParam && integrations) {
      if (integrations.some(i => i.id === integrationParam)) {
        setSelectedIntegrationId(integrationParam);
        setRunMode('single');
      }
    }
  }, [integrationParam, integrations]);

  // Get unique groups
  const groups = Array.from(
    new Set(integrations?.filter(i => i.groupName).map(i => i.groupName))
  ).filter(Boolean) as string[];

  const selectedIntegration = integrations?.find(i => i.id === selectedIntegrationId);

  const handleRun = async () => {
    if (runMode === 'single') {
      if (!selectedIntegrationId) {
        toast.error('Lütfen bir integration seçin');
        return;
      }

      setRunResult(null);
      setBatchResult(null);

      try {
        const result = await runMutation.mutateAsync(selectedIntegrationId);
        setRunResult(result);

        if (result.success) {
          toast.success(`Integration başarıyla çalıştırıldı! ${result.rowsAffected} satır etkilendi.`);
        } else {
          toast.error(result.error || 'Integration çalıştırılamadı');
        }
      } catch (error) {
        console.error('Run error:', error);
      }
    } else if (runMode === 'group') {
      if (!selectedGroup) {
        toast.error('Lütfen bir grup seçin');
        return;
      }

      setRunResult(null);
      setBatchResult(null);

      const groupIntegrations = integrations?.filter(i => i.groupName === selectedGroup) || [];
      const ids = groupIntegrations.map(i => i.id);

      try {
        const result = await batchRunMutation.mutateAsync(ids);
        setBatchResult(result);
      } catch (error) {
        console.error('Batch run error:', error);
      }
    } else if (runMode === 'batch') {
      if (selectedIntegrationIds.length === 0) {
        toast.error('Lütfen en az bir integration seçin');
        return;
      }

      setRunResult(null);
      setBatchResult(null);

      try {
        const result = await batchRunMutation.mutateAsync(selectedIntegrationIds);
        setBatchResult(result);
      } catch (error) {
        console.error('Batch run error:', error);
      }
    }
  };

  const toggleIntegrationSelection = (id: string) => {
    setSelectedIntegrationIds(prev =>
      prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]
    );
  };

  const isRunning = runMutation.isPending || batchRunMutation.isPending;

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div>
        <h1 className="text-4xl font-light text-foreground mb-2 flex items-center gap-3">
          <Play className="w-8 h-8" />
          Integration Çalıştır
        </h1>
        <p className="text-muted-foreground font-light">
          Tek integration, grup veya çoklu seçim ile veri aktarımı yapın
        </p>
      </div>

      {/* Run Mode Buttons */}
      <div className="flex gap-2">
        <Button
          variant={runMode === 'single' ? 'default' : 'outline'}
          onClick={() => {
            setRunMode('single');
            setSelectedIntegrationIds([]);
            setSelectedGroup(null);
            setRunResult(null);
            setBatchResult(null);
          }}
          className="zen-button"
        >
          <Play className="mr-2 h-4 w-4" />
          Tek Integration
        </Button>
        <Button
          variant={runMode === 'group' ? 'default' : 'outline'}
          onClick={() => {
            setRunMode('group');
            setSelectedIntegrationId(null);
            setSelectedIntegrationIds([]);
            setRunResult(null);
            setBatchResult(null);
          }}
          className="zen-button"
        >
          <Users className="mr-2 h-4 w-4" />
          Grup Çalıştır
        </Button>
        <Button
          variant={runMode === 'batch' ? 'default' : 'outline'}
          onClick={() => {
            setRunMode('batch');
            setSelectedIntegrationId(null);
            setSelectedGroup(null);
            setRunResult(null);
            setBatchResult(null);
          }}
          className="zen-button"
        >
          <CheckCircle2 className="mr-2 h-4 w-4" />
          Çoklu Seçim
        </Button>
      </div>

      {/* Main Card */}
      <Card className="zen-card max-w-3xl">
        <CardHeader>
          <CardTitle className="text-2xl font-light flex items-center gap-2">
            <Database className="w-6 h-6" />
            {runMode === 'single' && 'Integration Seç'}
            {runMode === 'group' && 'Grup Seç'}
            {runMode === 'batch' && 'Integration Seç (Çoklu)'}
          </CardTitle>
          <CardDescription className="font-light">
            {runMode === 'single' && 'Çalıştırmak için bir integration seçin'}
            {runMode === 'group' && 'Gruptaki tüm integration\'lar sırayla çalıştırılacak (transactional)'}
            {runMode === 'batch' && 'Seçili integration\'lar sırayla çalıştırılacak (transactional)'}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {isLoadingIntegrations ? (
            <div className="flex items-center gap-2 text-muted-foreground">
              <Loader2 className="w-4 h-4 animate-spin" />
              <span className="text-sm font-light">Integration'lar yükleniyor...</span>
            </div>
          ) : !integrations || integrations.length === 0 ? (
            <p className="text-sm text-muted-foreground font-light">
              Henüz integration yok. Lütfen önce bir integration oluşturun.
            </p>
          ) : (
            <>
              {/* Single Mode */}
              {runMode === 'single' && (
                <div className="space-y-3">
                  <label className="text-sm font-normal text-foreground">Integration</label>
                  <Select
                    value={selectedIntegrationId || undefined}
                    onValueChange={(value) => {
                      setSelectedIntegrationId(value);
                      setRunResult(null);
                    }}
                  >
                    <SelectTrigger className="zen-input h-12">
                      <SelectValue placeholder="Bir integration seçin..." />
                    </SelectTrigger>
                    <SelectContent>
                      {integrations.map((integration) => (
                        <SelectItem key={integration.id} value={integration.id}>
                          <div className="flex flex-col">
                            <div className="flex items-center gap-2">
                              <span className="font-normal">{integration.name}</span>
                              {integration.groupName && (
                                <Badge variant="secondary" className="text-xs">
                                  {integration.groupName}
                                </Badge>
                              )}
                            </div>
                            <span className="text-xs text-muted-foreground font-light">
                              {integration.sourceConnectionName} → {integration.targetConnectionName}
                            </span>
                          </div>
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              )}

              {/* Group Mode */}
              {runMode === 'group' && (
                <div className="space-y-3">
                  {groups.length === 0 ? (
                    <p className="text-sm text-muted-foreground font-light">
                      Henüz grup tanımlanmamış. Integration oluştururken grup adı verin.
                    </p>
                  ) : (
                    <>
                      <label className="text-sm font-normal text-foreground">Grup</label>
                      <Select
                        value={selectedGroup || undefined}
                        onValueChange={(value) => {
                          setSelectedGroup(value);
                          setBatchResult(null);
                        }}
                      >
                        <SelectTrigger className="zen-input h-12">
                          <SelectValue placeholder="Bir grup seçin..." />
                        </SelectTrigger>
                        <SelectContent>
                          {groups.map((group) => {
                            const count = integrations?.filter(i => i.groupName === group).length || 0;
                            return (
                              <SelectItem key={group} value={group}>
                                {group} ({count} integration)
                              </SelectItem>
                            );
                          })}
                        </SelectContent>
                      </Select>

                      {selectedGroup && (
                        <div className="p-4 rounded-xl bg-muted/30 space-y-2 border border-border">
                          <h4 className="text-sm font-normal text-foreground">Çalışma Sırası</h4>
                          <ul className="space-y-1">
                            {integrations
                              ?.filter(i => i.groupName === selectedGroup)
                              .sort((a, b) => a.executionOrder - b.executionOrder)
                              .map((i, idx) => (
                                <li key={i.id} className="text-xs text-muted-foreground font-light">
                                  {idx + 1}. {i.name} <span className="text-[10px]">(order: {i.executionOrder})</span>
                                </li>
                              ))}
                          </ul>
                        </div>
                      )}
                    </>
                  )}
                </div>
              )}

              {/* Batch Mode */}
              {runMode === 'batch' && (
                <div className="space-y-3">
                  <label className="text-sm font-normal text-foreground">Integration'lar</label>
                  <div className="space-y-2 max-h-64 overflow-y-auto border rounded-xl p-4 bg-muted/30">
                    {integrations.map((integration) => (
                      <div key={integration.id} className="flex items-center space-x-2">
                        <Checkbox
                          id={integration.id}
                          checked={selectedIntegrationIds.includes(integration.id)}
                          onCheckedChange={() => toggleIntegrationSelection(integration.id)}
                        />
                        <label htmlFor={integration.id} className="text-sm cursor-pointer flex-1">
                          {integration.name}
                          {integration.groupName && (
                            <Badge variant="secondary" className="ml-2 text-xs">
                              {integration.groupName} (#{integration.executionOrder})
                            </Badge>
                          )}
                        </label>
                      </div>
                    ))}
                  </div>
                  <p className="text-xs text-muted-foreground font-light">
                    Seçili: {selectedIntegrationIds.length} integration
                  </p>
                </div>
              )}

              {/* Selected Integration Details (Single Mode) */}
              {runMode === 'single' && selectedIntegration && (
                <div className="p-4 rounded-xl bg-muted/30 space-y-2 border border-border">
                  <h4 className="text-sm font-normal text-foreground mb-3">Integration Detayları</h4>
                  <div className="space-y-2 text-xs">
                    <div className="flex items-start gap-2">
                      <span className="text-muted-foreground font-light min-w-[120px]">Kaynak Sorgu:</span>
                      <code className="flex-1 text-foreground font-mono bg-background/50 px-2 py-1 rounded text-[11px]">
                        {selectedIntegration.sourceQuery}
                      </code>
                    </div>
                    <div className="flex items-start gap-2">
                      <span className="text-muted-foreground font-light min-w-[120px]">Hedef Sorgu:</span>
                      <code className="flex-1 text-foreground font-mono bg-background/50 px-2 py-1 rounded text-[11px]">
                        {selectedIntegration.targetQuery}
                      </code>
                    </div>
                  </div>
                </div>
              )}

              {/* Run Button */}
              <Button
                onClick={handleRun}
                disabled={
                  (runMode === 'single' && !selectedIntegrationId) ||
                  (runMode === 'group' && !selectedGroup) ||
                  (runMode === 'batch' && selectedIntegrationIds.length === 0) ||
                  isRunning
                }
                className="w-full zen-button h-12 text-base"
                size="lg"
              >
                {isRunning ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin mr-2" />
                    {runMode === 'single' ? 'Çalıştırılıyor...' : 'Batch Run Çalıştırılıyor...'}
                  </>
                ) : (
                  <>
                    <Play className="w-5 h-5 mr-2" />
                    {runMode === 'single' ? 'Şimdi Çalıştır' : 'Batch Run (Transactional)'}
                  </>
                )}
              </Button>

              {/* Progress Indicator */}
              {isRunning && (
                <div className="flex flex-col items-center gap-3 py-6">
                  <div className="relative">
                    <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
                      <Activity className="w-8 h-8 text-primary animate-pulse" />
                    </div>
                    <div className="absolute inset-0 rounded-full border-4 border-primary/20 border-t-primary animate-spin"></div>
                  </div>
                  <p className="text-sm text-muted-foreground font-light">
                    Veri transfer ediliyor...
                  </p>
                </div>
              )}

              {/* Single Run Result */}
              {runResult && !isRunning && (
                <Card className={`border-2 transition-all duration-400 ${
                  runResult.success 
                    ? 'border-green-500/50 bg-green-50/50 dark:bg-green-950/20' 
                    : 'border-red-500/50 bg-red-50/50 dark:bg-red-950/20'
                }`}>
                  <CardContent className="pt-6">
                    <div className="flex items-start gap-4">
                      <div className={`w-12 h-12 rounded-xl flex items-center justify-center flex-shrink-0 ${
                        runResult.success ? 'bg-green-500/10' : 'bg-red-500/10'
                      }`}>
                        {runResult.success ? (
                          <CheckCircle2 className="w-7 h-7 text-green-600 dark:text-green-400" />
                        ) : (
                          <XCircle className="w-7 h-7 text-red-600 dark:text-red-400" />
                        )}
                      </div>

                      <div className="flex-1 space-y-3">
                        <div>
                          <h3 className={`text-xl font-normal mb-1 ${
                            runResult.success 
                              ? 'text-green-700 dark:text-green-300' 
                              : 'text-red-700 dark:text-red-300'
                          }`}>
                            {runResult.success ? 'Integration Başarıyla Tamamlandı' : 'Integration Başarısız'}
                          </h3>
                          {runResult.message && (
                            <p className="text-sm text-muted-foreground font-light">{runResult.message}</p>
                          )}
                          {runResult.error && (
                            <p className="text-sm text-red-600 dark:text-red-400 font-light">{runResult.error}</p>
                          )}
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                          <div className="p-3 rounded-lg bg-background/50 border border-border">
                            <div className="flex items-center gap-2 mb-1">
                              <Database className="w-4 h-4 text-muted-foreground" />
                              <span className="text-xs text-muted-foreground font-light">Etkilenen Satır</span>
                            </div>
                            <p className="text-2xl font-light text-foreground">
                              {runResult.rowsAffected.toLocaleString()}
                            </p>
                          </div>

                          <div className="p-3 rounded-lg bg-background/50 border border-border">
                            <div className="flex items-center gap-2 mb-1">
                              <Clock className="w-4 h-4 text-muted-foreground" />
                              <span className="text-xs text-muted-foreground font-light">Süre</span>
                            </div>
                            <p className="text-2xl font-light text-foreground">
                              {runResult.durationMs < 1000 
                                ? `${runResult.durationMs}ms` 
                                : `${(runResult.durationMs / 1000).toFixed(2)}s`}
                            </p>
                          </div>
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}

              {/* Batch Run Result */}
              {batchResult && !isRunning && (
                <Card className={`border-2 transition-all duration-400 ${
                  batchResult.success 
                    ? 'border-green-500/50 bg-green-50/50 dark:bg-green-950/20' 
                    : 'border-red-500/50 bg-red-50/50 dark:bg-red-950/20'
                }`}>
                  <CardContent className="pt-6 space-y-4">
                    <div className="flex items-start gap-4">
                      <div className={`w-12 h-12 rounded-xl flex items-center justify-center flex-shrink-0 ${
                        batchResult.success ? 'bg-green-500/10' : 'bg-red-500/10'
                      }`}>
                        {batchResult.success ? (
                          <CheckCircle2 className="w-7 h-7 text-green-600 dark:text-green-400" />
                        ) : (
                          <XCircle className="w-7 h-7 text-red-600 dark:text-red-400" />
                        )}
                      </div>

                      <div className="flex-1 space-y-3">
                        <div>
                          <h3 className={`text-xl font-normal mb-1 ${
                            batchResult.success 
                              ? 'text-green-700 dark:text-green-300' 
                              : 'text-red-700 dark:text-red-300'
                          }`}>
                            {batchResult.success ? 'Batch Run Başarılı' : 'Batch Run Başarısız (ROLLBACK)'}
                          </h3>
                          {batchResult.error && (
                            <p className="text-sm text-red-600 dark:text-red-400 font-light">{batchResult.error}</p>
                          )}
                        </div>

                        <div className="grid grid-cols-2 gap-4">
                          <div className="p-3 rounded-lg bg-background/50 border border-border">
                            <div className="flex items-center gap-2 mb-1">
                              <Database className="w-4 h-4 text-muted-foreground" />
                              <span className="text-xs text-muted-foreground font-light">Toplam Satır</span>
                            </div>
                            <p className="text-2xl font-light text-foreground">
                              {batchResult.totalRowsAffected.toLocaleString()}
                            </p>
                          </div>

                          <div className="p-3 rounded-lg bg-background/50 border border-border">
                            <div className="flex items-center gap-2 mb-1">
                              <Clock className="w-4 h-4 text-muted-foreground" />
                              <span className="text-xs text-muted-foreground font-light">Toplam Süre</span>
                            </div>
                            <p className="text-2xl font-light text-foreground">
                              {batchResult.totalDurationMs < 1000 
                                ? `${batchResult.totalDurationMs}ms` 
                                : `${(batchResult.totalDurationMs / 1000).toFixed(2)}s`}
                            </p>
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Individual Results */}
                    <div className="space-y-2">
                      <h4 className="font-normal text-sm">Detaylar:</h4>
                      <div className="space-y-2">
                        {batchResult.results.map((result, idx) => (
                          <div
                            key={result.integrationId}
                            className="flex items-center justify-between p-3 border rounded-lg bg-background/50"
                          >
                            <div className="flex items-center gap-2">
                              {result.success ? (
                                <CheckCircle2 className="h-4 w-4 text-green-500" />
                              ) : (
                                <XCircle className="h-4 w-4 text-red-500" />
                              )}
                              <span className="text-sm font-light">
                                {idx + 1}. {result.integrationName}
                              </span>
                            </div>
                            <div className="text-sm text-muted-foreground">
                              {result.success ? (
                                <span className="text-green-600 dark:text-green-400">{result.rowsAffected} satır</span>
                              ) : (
                                <span className="text-red-600 dark:text-red-400 text-xs">{result.error}</span>
                              )}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </CardContent>
                </Card>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {/* Info Card */}
      <Card className="zen-card max-w-3xl bg-primary/5 border-primary/20">
        <CardContent className="pt-6">
          <div className="flex items-start gap-3">
            <div className="w-8 h-8 rounded-lg bg-primary/10 flex items-center justify-center flex-shrink-0">
              <Activity className="w-5 h-5 text-primary" />
            </div>
            <div className="space-y-1">
              <h4 className="text-sm font-normal text-foreground">Batch Run (Transactional)</h4>
              <p className="text-xs text-muted-foreground font-light leading-relaxed">
                Grup veya çoklu seçim modunda integration'lar <strong>ExecutionOrder</strong>'a göre sırayla çalıştırılır. 
                Tüm işlemler bir <strong>transaction</strong> içinde yapılır - herhangi biri başarısız olursa tüm işlemler geri alınır (ROLLBACK).
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

export default function RunIntegrationPage() {
  return (
    <Suspense fallback={<div className="flex items-center justify-center min-h-[400px]"><Loader2 className="w-8 h-8 animate-spin" /></div>}>
      <RunIntegrationContent />
    </Suspense>
  );
}
