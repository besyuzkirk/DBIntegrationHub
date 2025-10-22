'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { ArrowRight, Loader2, Save, ArrowLeftRight } from 'lucide-react';
import { useIntegrations, useIntegrationColumns } from '@/hooks/useIntegrations';
import { useMappings, useSaveMappings } from '@/hooks/useMappings';

export default function MappingsPage() {
  const [selectedIntegrationId, setSelectedIntegrationId] = useState<string>('');
  const [mappings, setMappings] = useState<Record<string, string>>({});

  const { data: integrations, isLoading: integrationsLoading } = useIntegrations();
  const { data: columns, isLoading: columnsLoading } = useIntegrationColumns(selectedIntegrationId || undefined);
  const { data: savedMappings } = useMappings(selectedIntegrationId || undefined);
  const saveMappingsMutation = useSaveMappings();

  // Saved mappings yüklendiğinde state'e aktar
  useState(() => {
    if (savedMappings) {
      const mappingObj: Record<string, string> = {};
      savedMappings.forEach(m => {
        mappingObj[m.targetParameter] = m.sourceColumn;
      });
      setMappings(mappingObj);
    }
  });

  const handleIntegrationChange = (integrationId: string) => {
    setSelectedIntegrationId(integrationId);
    setMappings({});
  };

  const handleMapping = (targetParam: string, sourceCol: string) => {
    setMappings(prev => ({
      ...prev,
      [targetParam]: sourceCol
    }));
  };

  const handleSave = async () => {
    if (!selectedIntegrationId) return;

    const mappingArray = Object.entries(mappings).map(([targetParameter, sourceColumn]) => ({
      sourceColumn,
      targetParameter
    }));

    await saveMappingsMutation.mutateAsync({
      integrationId: selectedIntegrationId,
      mappings: mappingArray
    });
  };

  const isMapped = (targetParam: string) => !!mappings[targetParam];
  const allMapped = columns?.targetParameters.every(tp => isMapped(tp)) ?? false;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Column Mapping</h1>
        <p className="text-muted-foreground">
          Kaynak kolonlarını hedef parametrelerine eşleştirin
        </p>
      </div>

      {/* Integration Selection */}
      <Card>
        <CardHeader>
          <CardTitle>Integration Seç</CardTitle>
          <CardDescription>Mapping yapmak için bir integration seçin</CardDescription>
        </CardHeader>
        <CardContent>
          <Select value={selectedIntegrationId} onValueChange={handleIntegrationChange}>
            <SelectTrigger className="w-full">
              <SelectValue placeholder="Integration seçin..." />
            </SelectTrigger>
            <SelectContent>
              {integrationsLoading ? (
                <SelectItem value="loading" disabled>Yükleniyor...</SelectItem>
              ) : integrations?.length === 0 ? (
                <SelectItem value="empty" disabled>Integration bulunamadı</SelectItem>
              ) : (
                integrations?.map(int => (
                  <SelectItem key={int.id} value={int.id}>{int.name}</SelectItem>
                ))
              )}
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {/* Mapping Interface */}
      {selectedIntegrationId && (
        <>
          {columnsLoading ? (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : columns ? (
            <>
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <ArrowLeftRight className="w-5 h-5" />
                    Kolon Eşleştirme
                  </CardTitle>
                  <CardDescription>
                    Her hedef parametre için bir kaynak kolon seçin
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {columns.targetParameters.map((targetParam) => (
                      <div key={targetParam} className="flex items-center gap-4 p-4 border rounded-lg">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-2">
                            <Badge variant="secondary">Hedef</Badge>
                            <code className="text-sm font-mono">{targetParam}</code>
                          </div>
                          <Select
                            value={mappings[targetParam] || ''}
                            onValueChange={(val) => handleMapping(targetParam, val)}
                          >
                            <SelectTrigger>
                              <SelectValue placeholder="Kaynak kolon seç..." />
                            </SelectTrigger>
                            <SelectContent>
                              {columns.sourceColumns.map(col => (
                                <SelectItem key={col} value={col}>
                                  <div className="flex items-center gap-2">
                                    <Badge variant="outline" className="text-xs">Kaynak</Badge>
                                    <code className="text-sm">{col}</code>
                                  </div>
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        </div>
                        {isMapped(targetParam) && (
                          <div className="flex items-center gap-2 text-green-600">
                            <ArrowRight className="w-5 h-5" />
                            <span className="text-sm font-medium">Eşleşti</span>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>

                  <div className="mt-6 flex items-center justify-between">
                    <div className="text-sm text-muted-foreground">
                      {Object.keys(mappings).length} / {columns.targetParameters.length} parametre eşleştirildi
                    </div>
                    <Button
                      onClick={handleSave}
                      disabled={!allMapped || saveMappingsMutation.isPending}
                      size="lg"
                    >
                      {saveMappingsMutation.isPending ? (
                        <>
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                          Kaydediliyor...
                        </>
                      ) : (
                        <>
                          <Save className="mr-2 h-4 w-4" />
                          Mapping Kaydet
                        </>
                      )}
                    </Button>
                  </div>
                </CardContent>
              </Card>

              {/* Preview */}
              <Card className="bg-muted/50">
                <CardHeader>
                  <CardTitle className="text-lg">Mapping Özeti</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    {Object.entries(mappings).map(([target, source]) => (
                      <div key={target} className="flex items-center gap-3 text-sm">
                        <Badge variant="outline">{source}</Badge>
                        <ArrowRight className="w-4 h-4 text-muted-foreground" />
                        <Badge variant="secondary">{target}</Badge>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </>
          ) : (
            <Card>
              <CardContent className="py-12 text-center text-muted-foreground">
                Kolon bilgileri yüklenemedi
              </CardContent>
            </Card>
          )}
        </>
      )}
    </div>
  );
}
