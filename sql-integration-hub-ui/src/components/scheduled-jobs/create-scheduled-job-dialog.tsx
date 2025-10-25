'use client';

import { useState } from 'react';
import { useScheduledJobs } from '@/hooks/useScheduledJobs';
import { useIntegrations } from '@/hooks/useIntegrations';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';

interface CreateScheduledJobDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  preSelectedIntegrationId?: string;
}

export function CreateScheduledJobDialog({
  open,
  onOpenChange,
  preSelectedIntegrationId,
}: CreateScheduledJobDialogProps) {
  const { createScheduledJob } = useScheduledJobs();
  const { data: integrations } = useIntegrations();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [cronExpression, setCronExpression] = useState('0 0 * * *'); // Her gün gece yarısı
  const [jobType, setJobType] = useState<'integration' | 'group'>(
    preSelectedIntegrationId ? 'integration' : 'integration'
  );
  const [selectedIntegrationId, setSelectedIntegrationId] = useState(
    preSelectedIntegrationId || ''
  );
  const [selectedGroupId, setSelectedGroupId] = useState('');

  // Grup isimlerini unique olarak al
  const groupNames = Array.from(
    new Set(
      integrations
        ?.filter((i: any) => i.groupName)
        .map((i: any) => i.groupName)
    )
  );

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      await createScheduledJob({
        name,
        description,
        cronExpression,
        integrationId:
          jobType === 'integration' ? selectedIntegrationId : undefined,
        groupId: jobType === 'group' ? selectedGroupId : undefined,
      });

      // Form'u temizle
      setName('');
      setDescription('');
      setCronExpression('0 0 * * *');
      setSelectedIntegrationId('');
      setSelectedGroupId('');
      onOpenChange(false);
    } catch (error) {
      // Hata toast'ı hook tarafından gösterilecek
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>Yeni Zamanlanmış İş</DialogTitle>
          <DialogDescription>
            Integration veya grup için zamanlanmış iş oluşturun
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">İş Adı</Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Günlük Satış Raporu"
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Açıklama</Label>
            <Textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Her gün gece yarısı satış verilerini senkronize et"
              rows={3}
            />
          </div>

          <div className="space-y-2">
            <Label>İş Tipi</Label>
            <RadioGroup
              value={jobType}
              onValueChange={(value: 'integration' | 'group') =>
                setJobType(value)
              }
            >
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="integration" id="integration" />
                <Label htmlFor="integration" className="cursor-pointer">
                  Tek Integration
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="group" id="group" />
                <Label htmlFor="group" className="cursor-pointer">
                  Integration Grubu
                </Label>
              </div>
            </RadioGroup>
          </div>

          {jobType === 'integration' && (
            <div className="space-y-2">
              <Label htmlFor="integration">Integration</Label>
              <Select
                value={selectedIntegrationId}
                onValueChange={setSelectedIntegrationId}
                required
              >
                <SelectTrigger>
                  <SelectValue placeholder="Integration seçin" />
                </SelectTrigger>
                <SelectContent>
                  {integrations?.map((integration: any) => (
                    <SelectItem key={integration.id} value={integration.id}>
                      {integration.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}

          {jobType === 'group' && (
            <div className="space-y-2">
              <Label htmlFor="group">Grup</Label>
              <Select
                value={selectedGroupId}
                onValueChange={setSelectedGroupId}
                required
              >
                <SelectTrigger>
                  <SelectValue placeholder="Grup seçin" />
                </SelectTrigger>
                <SelectContent>
                  {groupNames.map((groupName: any) => (
                    <SelectItem key={groupName} value={groupName}>
                      {groupName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          )}

          <div className="space-y-2">
            <Label htmlFor="cron">Cron İfadesi</Label>
            <Input
              id="cron"
              value={cronExpression}
              onChange={(e) => setCronExpression(e.target.value)}
              placeholder="0 0 * * *"
              required
            />
            <p className="text-xs text-muted-foreground">
              <a
                href="https://crontab.guru/"
                target="_blank"
                rel="noopener noreferrer"
                className="underline"
              >
                Cron ifadesi yardımı
              </a>
              {' • '}
              <span>Örnekler:</span>
              <br />
              <code className="text-xs">0 0 * * *</code> - Her gün gece yarısı
              <br />
              <code className="text-xs">0 */6 * * *</code> - Her 6 saatte bir
              <br />
              <code className="text-xs">0 0 * * 0</code> - Her Pazar gece yarısı
            </p>
          </div>

          <div className="flex justify-end gap-2 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              İptal
            </Button>
            <Button type="submit">Oluştur</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

