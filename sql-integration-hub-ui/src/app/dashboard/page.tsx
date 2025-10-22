'use client';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useDashboardStatistics, useRecentLogs, useIntegrationActivity } from '@/hooks/useDashboard';
import { Skeleton } from '@/components/ui/skeleton';
import { Database, GitBranch, Map, FileText, CheckCircle2, XCircle, Activity, TrendingUp, TrendingDown } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { tr } from 'date-fns/locale';
import { PasswordWarningBanner } from '@/components/auth/password-warning-banner';

export default function DashboardPage() {
  const { data: stats, isLoading: statsLoading } = useDashboardStatistics();
  const { data: recentLogs, isLoading: logsLoading } = useRecentLogs(8);
  const { data: activity, isLoading: activityLoading } = useIntegrationActivity(7);

  return (
    <div className="space-y-8 animate-fade-in">
      {/* Password Warning for Admin */}
      <PasswordWarningBanner />
      
      {/* Header */}
      <div>
        <h1 className="text-4xl font-light text-foreground mb-2">
          Dashboard
        </h1>
        <p className="text-muted-foreground font-light">
          Veritabanı entegrasyon merkezinize hoş geldiniz
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        {statsLoading ? (
          Array.from({ length: 4 }).map((_, i) => (
            <Card key={i} className="zen-card">
              <CardHeader className="pb-2">
                <Skeleton className="h-4 w-24" />
              </CardHeader>
              <CardContent>
                <Skeleton className="h-9 w-16 mb-2" />
                <Skeleton className="h-3 w-20" />
              </CardContent>
            </Card>
          ))
        ) : (
          <>
            <StatCard
              icon={<Database className="w-5 h-5 text-blue-500" />}
              label="Toplam Bağlantı"
              value={stats?.totalConnections || 0}
              change={`${stats?.activeConnections || 0} aktif, ${stats?.inactiveConnections || 0} pasif`}
            />
            <StatCard
              icon={<GitBranch className="w-5 h-5 text-green-500" />}
              label="Entegrasyonlar"
              value={stats?.totalIntegrations || 0}
              change={`Toplam ${stats?.totalLogs || 0} çalıştırma`}
            />
            <StatCard
              icon={<Map className="w-5 h-5 text-purple-500" />}
              label="Veri Eşleştirmeleri"
              value={stats?.totalMappings || 0}
              change="Alan eşleştirmeleri"
            />
            <StatCard
              icon={<FileText className="w-5 h-5 text-orange-500" />}
              label="Bugünkü Çalıştırmalar"
              value={stats?.todayLogs || 0}
              change={`${stats?.successfulLogsToday || 0} başarılı, ${stats?.failedLogsToday || 0} başarısız`}
            />
          </>
        )}
      </div>

      {/* Success Rate Cards */}
      <div className="grid gap-6 md:grid-cols-2">
        {statsLoading ? (
          Array.from({ length: 2 }).map((_, i) => (
            <Card key={i} className="zen-card">
              <CardHeader className="pb-2">
                <Skeleton className="h-5 w-32" />
              </CardHeader>
              <CardContent>
                <Skeleton className="h-12 w-20" />
              </CardContent>
            </Card>
          ))
        ) : (
          <>
            <Card className="zen-card">
              <CardHeader className="pb-2">
                <CardDescription className="font-light flex items-center gap-2">
                  <Activity className="w-4 h-4" />
                  Bugünkü Başarı Oranı
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-baseline gap-2">
                  <div className="text-4xl font-light text-foreground">
                    {stats?.successRateToday?.toFixed(1) || 0}%
                  </div>
                  {stats && stats.successRateToday >= 90 ? (
                    <TrendingUp className="w-5 h-5 text-green-500" />
                  ) : (
                    <TrendingDown className="w-5 h-5 text-red-500" />
                  )}
                </div>
              </CardContent>
            </Card>

            <Card className="zen-card">
              <CardHeader className="pb-2">
                <CardDescription className="font-light flex items-center gap-2">
                  <Activity className="w-4 h-4" />
                  Genel Başarı Oranı
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex items-baseline gap-2">
                  <div className="text-4xl font-light text-foreground">
                    {stats?.overallSuccessRate?.toFixed(1) || 0}%
                  </div>
                  {stats && stats.overallSuccessRate >= 90 ? (
                    <TrendingUp className="w-5 h-5 text-green-500" />
                  ) : (
                    <TrendingDown className="w-5 h-5 text-red-500" />
                  )}
                </div>
              </CardContent>
            </Card>
          </>
        )}
      </div>

      {/* Recent Logs */}
      <Card className="zen-card">
        <CardHeader>
          <CardTitle className="text-2xl font-light">
            Son Entegrasyon Logları
          </CardTitle>
          <CardDescription className="font-light">
            En son çalıştırılan entegrasyonlar
          </CardDescription>
        </CardHeader>
        <CardContent>
          {logsLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : recentLogs && recentLogs.length > 0 ? (
            <div className="space-y-3">
              {recentLogs.map((log) => (
                <div
                  key={log.id}
                  className="flex items-center justify-between p-4 rounded-lg border border-border/50 hover:bg-accent/5 transition-colors"
                >
                  <div className="flex items-center gap-4 flex-1">
                    {log.success ? (
                      <CheckCircle2 className="w-5 h-5 text-green-500 flex-shrink-0" />
                    ) : (
                      <XCircle className="w-5 h-5 text-red-500 flex-shrink-0" />
                    )}
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-foreground truncate">
                        {log.integrationName}
                      </p>
                      <p className="text-sm text-muted-foreground">
                        {log.rowCount} satır • {log.durationMs}ms
                        {log.message && ` • ${log.message}`}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    <Badge variant={log.success ? 'default' : 'destructive'}>
                      {log.success ? 'Başarılı' : 'Başarısız'}
                    </Badge>
                    <p className="text-xs text-muted-foreground whitespace-nowrap">
                      {formatDistanceToNow(new Date(log.runDate), { addSuffix: true, locale: tr })}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <FileText className="w-12 h-12 mx-auto mb-2 opacity-50" />
              <p>Henüz entegrasyon logu yok</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Activity Chart */}
      {!activityLoading && activity && activity.length > 0 && (
        <Card className="zen-card">
          <CardHeader>
            <CardTitle className="text-2xl font-light">
              Son 7 Günlük Aktivite
            </CardTitle>
            <CardDescription className="font-light">
              Günlük entegrasyon çalıştırma istatistikleri
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              {activity.map((day) => (
                <div key={day.date} className="flex items-center gap-4">
                  <div className="w-24 text-sm text-muted-foreground">
                    {new Date(day.date).toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })}
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center gap-2 mb-1">
                      <div className="flex-1 h-6 bg-muted rounded-full overflow-hidden">
                        {day.totalRuns > 0 && (
                          <div
                            className="h-full bg-green-500"
                            style={{ width: `${day.successRate}%` }}
                          />
                        )}
                      </div>
                      <span className="text-sm font-medium w-12 text-right">
                        {day.successRate.toFixed(0)}%
                      </span>
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {day.totalRuns} çalıştırma ({day.successfulRuns} başarılı, {day.failedRuns} başarısız)
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function StatCard({ icon, label, value, change }: { icon: React.ReactNode; label: string; value: number; change: string }) {
  return (
    <Card className="zen-card">
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardDescription className="font-light">{label}</CardDescription>
          {icon}
        </div>
      </CardHeader>
      <CardContent>
        <div className="text-3xl font-light text-foreground">{value}</div>
        <p className="text-xs text-muted-foreground font-light mt-1">
          {change}
        </p>
      </CardContent>
    </Card>
  );
}

