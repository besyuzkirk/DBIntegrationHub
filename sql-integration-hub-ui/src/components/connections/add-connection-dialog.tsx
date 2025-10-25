'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useCreateConnection, useTestConnection, CreateConnectionRequest } from '@/hooks/useConnections';
import { Plus, TestTube, Search } from 'lucide-react';

interface FormData {
  name: string;
  databaseType: 'PostgreSQL' | 'MySQL' | 'SQLServer' | 'MongoDB';
  connectionString: string;
  // Ayrı alanlar
  host: string;
  port: string;
  database: string;
  username: string;
  password: string;
  // MongoDB için özel alanlar
  mongoConnectionString: string;
}

interface ConnectionFields {
  host: string;
  port: string;
  database: string;
  username: string;
  password: string;
}

export function AddConnectionDialog() {
  const [open, setOpen] = useState(false);
  const [inputMode, setInputMode] = useState<'string' | 'fields'>('fields'); // Varsayılan olarak ayrı alanlar
  const [isDiscovering, setIsDiscovering] = useState(false);
  const [discoveredServers, setDiscoveredServers] = useState<string[]>([]);
  const [showServerList, setShowServerList] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
    setValue,
  } = useForm<FormData>();

  const createMutation = useCreateConnection();
  const testMutation = useTestConnection();

  const dbType = watch('databaseType');
  const connectionString = watch('connectionString');
  const host = watch('host');
  const port = watch('port');
  const database = watch('database');
  const username = watch('username');
  const password = watch('password');
  const mongoConnectionString = watch('mongoConnectionString');

  // Veritabanı tipi değiştiğinde alanları temizle
  const handleDatabaseTypeChange = (newDbType: string) => {
    setValue('databaseType', newDbType as 'PostgreSQL' | 'MySQL' | 'SQLServer' | 'MongoDB');
    
    // Mevcut alanları temizle
    setValue('host', '');
    setValue('port', '');
    setValue('database', '');
    setValue('username', '');
    setValue('password', '');
    setValue('mongoConnectionString', '');
    
    // Server keşfi state'lerini temizle
    setDiscoveredServers([]);
    setShowServerList(false);
  };

  // Server keşfi fonksiyonu - Optimize edilmiş
  const handleDiscoverServers = async () => {
    if (!dbType) return;
    
    setIsDiscovering(true);
    setDiscoveredServers([]);
    setShowServerList(false);
    
    try {
      // Sadece yaygın portları kontrol et
      const commonPorts = getCommonPorts(dbType);
      const discovered: string[] = [];
      
      // Localhost ve yaygın IP aralıklarını kontrol et
      const ipRanges = ['localhost', '127.0.0.1'];
      
      // Yerel ağ IP aralığını ekle (daha geniş tarama)
      for (let i = 1; i <= 50; i++) {
        ipRanges.push(`192.168.1.${i}`);
        ipRanges.push(`192.168.0.${i}`);
        ipRanges.push(`10.0.0.${i}`);
        ipRanges.push(`172.16.0.${i}`);
      }
      
      // Paralel kontrol yap
      const promises = [];
      
      for (const ip of ipRanges) {
        for (const port of commonPorts) {
          promises.push(
            checkServerReachability(ip, port, dbType)
              .then(isReachable => {
                if (isReachable) {
                  discovered.push(`${ip}:${port}`);
                }
              })
              .catch(() => {
                // Sessizce devam et
              })
          );
        }
      }
      
      // Tüm kontrolleri paralel olarak yap
      await Promise.allSettled(promises);
      
      setDiscoveredServers(discovered);
      setShowServerList(true);
    } catch (error) {
      console.error('Server keşfi hatası:', error);
    } finally {
      setIsDiscovering(false);
    }
  };

  // Veritabanı tipine göre yaygın portları döndür - Genişletilmiş
  const getCommonPorts = (dbType: string): number[] => {
    switch (dbType) {
      case 'PostgreSQL':
        return [5432, 5433, 5434]; // Default + alternatif portlar
      case 'MySQL':
        return [3306, 3307, 3308]; // Default + alternatif portlar
      case 'SQLServer':
        return [1433, 1434, 1435]; // Default + alternatif portlar
      case 'MongoDB':
        return [27017, 27018, 27019]; // Default + alternatif portlar
      default:
        return [5432, 3306, 1433, 27017]; // Tüm default portlar
    }
  };

  // Server erişilebilirliğini kontrol et - Gerçek veritabanı testi
  const checkServerReachability = async (ip: string, port: number, dbType: string): Promise<boolean> => {
    return new Promise((resolve) => {
      const timeout = 2000; // 2 saniye timeout
      
      // Gerçek veritabanı bağlantı testi yap
      const testConnectionString = buildTestConnectionString(dbType, ip, port);
      
      if (!testConnectionString) {
        resolve(false);
        return;
      }
      
      // API üzerinden bağlantı testi yap
      fetch('/api/connections/test', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          connectionString: testConnectionString,
          databaseType: dbType
        })
      })
      .then(response => {
        if (response.ok) {
          resolve(true);
        } else {
          resolve(false);
        }
      })
      .catch(() => {
        resolve(false);
      });
    });
  };

  // Test için basit connection string oluştur
  const buildTestConnectionString = (dbType: string, host: string, port: number): string => {
    switch (dbType) {
      case 'PostgreSQL':
        return `Host=${host};Port=${port};Database=postgres;Username=postgres;Password=password`;
      case 'MySQL':
        return `Server=${host};Port=${port};Database=mysql;Uid=root;Pwd=password`;
      case 'SQLServer':
        return `Server=${host},${port};Database=master;User Id=sa;Password=password;TrustServerCertificate=true;Connect Timeout=5`;
      case 'MongoDB':
        return `mongodb://admin:password@${host}:${port}/admin`;
      default:
        return '';
    }
  };

  // Keşfedilen server'ı seç
  const handleSelectServer = (server: string) => {
    const [host, port] = server.split(':');
    setValue('host', host);
    setValue('port', port);
    setShowServerList(false);
  };

  const onSubmit = async (data: FormData) => {
    // Ayrı alanlardan connection string oluştur
    if (inputMode === 'fields') {
      if (dbType === 'MongoDB') {
        data.connectionString = mongoConnectionString;
      } else {
        data.connectionString = buildConnectionString(dbType, {
          host: data.host,
          port: data.port,
          database: data.database,
          username: data.username,
          password: data.password
        });
      }
    }
    
    await createMutation.mutateAsync(data);
    setOpen(false);
    reset();
  };

  const handleTestConnection = async () => {
    if (!dbType) return;
    
    let testConnectionString = connectionString;
    
    // Ayrı alanlardan connection string oluştur
    if (inputMode === 'fields') {
      if (dbType === 'MongoDB') {
        testConnectionString = mongoConnectionString;
      } else {
        testConnectionString = buildConnectionString(dbType, {
          host: host || '',
          port: port || '',
          database: database || '',
          username: username || '',
          password: password || ''
        });
      }
    }
    
    if (!testConnectionString) return;
    
    await testMutation.mutateAsync({
      connectionString: testConnectionString,
      databaseType: dbType,
    });
  };

  const getConnectionStringPlaceholder = (dbType: string) => {
    switch (dbType) {
      case 'PostgreSQL':
        return 'Host=localhost;Database=postgres;Username=postgres;Password=***';
      case 'MySQL':
        return 'Server=localhost;Database=mysql;Uid=root;Pwd=***';
      case 'SQLServer':
        return 'Server=localhost\\SQLEXPRESS;Database=master;User Id=sa;Password=***';
      case 'MongoDB':
        return 'mongodb://admin:password@localhost:27017/mydb';
      default:
        return 'Enter connection string...';
    }
  };

  const getConnectionStringExample = (dbType: string) => {
    switch (dbType) {
      case 'PostgreSQL':
        return 'Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=password';
      case 'MySQL':
        return 'Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=password';
      case 'SQLServer':
        return 'Server=localhost;Database=mydb;User Id=sa;Password=password;TrustServerCertificate=true';
      case 'MongoDB':
        return 'mongodb://username:password@localhost:27017/mydb';
      default:
        return '';
    }
  };

  const getConnectionStringFormat = (dbType: string) => {
    switch (dbType) {
      case 'PostgreSQL':
        return 'Host=server;Port=5432;Database=dbname;Username=user;Password=pass';
      case 'MySQL':
        return 'Server=server;Port=3306;Database=dbname;Uid=user;Pwd=pass';
      case 'SQLServer':
        return 'Server=server;Database=dbname;User Id=user;Password=pass;TrustServerCertificate=true';
      case 'MongoDB':
        return 'mongodb://user:pass@server:27017/dbname';
      default:
        return '';
    }
  };

  const buildConnectionString = (dbType: string, fields: ConnectionFields): string => {
    const { host, port, database, username, password } = fields;
    
    if (!host || !database || !username || !password) {
      return '';
    }

    switch (dbType) {
      case 'PostgreSQL':
        return `Host=${host};Port=${port || '5432'};Database=${database};Username=${username};Password=${password}`;
      case 'MySQL':
        return `Server=${host};Port=${port || '3306'};Database=${database};Uid=${username};Pwd=${password}`;
      case 'SQLServer':
        return `Server=${host};Database=${database};User Id=${username};Password=${password};TrustServerCertificate=true`;
      default:
        return '';
    }
  };

  const parseConnectionString = (connectionString: string, dbType: string): ConnectionFields => {
    const fields: ConnectionFields = {
      host: '',
      port: '',
      database: '',
      username: '',
      password: ''
    };

    if (!connectionString) return fields;

    const pairs = connectionString.split(';').filter(pair => pair.trim());
    
    for (const pair of pairs) {
      const [key, value] = pair.split('=').map(s => s.trim());
      if (!key || !value) continue;

      switch (dbType) {
        case 'PostgreSQL':
          if (key.toLowerCase() === 'host') fields.host = value;
          else if (key.toLowerCase() === 'port') fields.port = value;
          else if (key.toLowerCase() === 'database') fields.database = value;
          else if (key.toLowerCase() === 'username') fields.username = value;
          else if (key.toLowerCase() === 'password') fields.password = value;
          break;
        case 'MySQL':
          if (key.toLowerCase() === 'server') fields.host = value;
          else if (key.toLowerCase() === 'port') fields.port = value;
          else if (key.toLowerCase() === 'database') fields.database = value;
          else if (key.toLowerCase() === 'uid') fields.username = value;
          else if (key.toLowerCase() === 'pwd') fields.password = value;
          break;
        case 'SQLServer':
          if (key.toLowerCase() === 'server') fields.host = value;
          else if (key.toLowerCase() === 'database') fields.database = value;
          else if (key.toLowerCase() === 'user id') fields.username = value;
          else if (key.toLowerCase() === 'password') fields.password = value;
          break;
      }
    }

    return fields;
  };

  const handleModeToggle = () => {
    if (inputMode === 'string' && connectionString) {
      // String'den ayrı alanlara parse et
      const parsed = parseConnectionString(connectionString, dbType);
      setValue('host', parsed.host);
      setValue('port', parsed.port);
      setValue('database', parsed.database);
      setValue('username', parsed.username);
      setValue('password', parsed.password);
    } else if (inputMode === 'fields') {
      // Ayrı alanlardan string oluştur
      const built = buildConnectionString(dbType, {
        host: host || '',
        port: port || '',
        database: database || '',
        username: username || '',
        password: password || ''
      });
      setValue('connectionString', built);
    }
    
    setInputMode(inputMode === 'string' ? 'fields' : 'string');
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button className="zen-button">
          <Plus className="w-4 h-4 mr-2" />
          Add Connection
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle className="text-2xl font-light">Add New Connection</DialogTitle>
          <DialogDescription className="font-light">
            Create a new database connection. Fill in the details below.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 py-4">
          {/* Name */}
          <div className="space-y-2">
            <Label htmlFor="name" className="text-sm font-normal">
              Connection Name
            </Label>
            <Input
              id="name"
              {...register('name', {
                required: 'Name is required',
                minLength: {
                  value: 3,
                  message: 'Name must be at least 3 characters',
                },
              })}
              placeholder="My Database Connection"
              className="zen-input"
            />
            {errors.name && (
              <p className="text-sm text-destructive font-light">
                {errors.name.message}
              </p>
            )}
          </div>

          {/* Database Type */}
          <div className="space-y-2">
            <Label htmlFor="databaseType" className="text-sm font-normal">
              Database Type
            </Label>
            <Select
              onValueChange={handleDatabaseTypeChange}
            >
              <SelectTrigger className="zen-input">
                <SelectValue placeholder="Select database type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="PostgreSQL">PostgreSQL</SelectItem>
                <SelectItem value="MySQL">MySQL</SelectItem>
                <SelectItem value="SQLServer">SQL Server</SelectItem>
                <SelectItem value="MongoDB">MongoDB</SelectItem>
              </SelectContent>
            </Select>
            {errors.databaseType && (
              <p className="text-sm text-destructive font-light">
                {errors.databaseType.message}
              </p>
            )}
          </div>

          {/* Connection String */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <Label htmlFor="connectionString" className="text-sm font-normal">
                Connection String
              </Label>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handleModeToggle}
                className="text-xs"
              >
                {inputMode === 'string' ? 'Ayrı Alanlar' : 'String Formatı'}
              </Button>
            </div>
            {inputMode === 'string' ? (
              <Input
                id="connectionString"
                {...register('connectionString', {
                  required: 'Connection string is required',
                  validate: (value) => {
                    if (!value) return 'Connection string is required';
                    
                    const dbType = watch('databaseType');
                    if (!dbType) return 'Please select database type first';
                    
                    // Basic format validation
                    if (dbType === 'PostgreSQL') {
                      const requiredKeys = ['Host', 'Database', 'Username', 'Password'];
                      const missingKeys = requiredKeys.filter(key => 
                        !value.toLowerCase().includes(key.toLowerCase() + '=')
                      );
                      if (missingKeys.length > 0) {
                        return `PostgreSQL connection string must include: ${missingKeys.join(', ')}`;
                      }
                    } else if (dbType === 'MySQL') {
                      const requiredKeys = ['Server', 'Database', 'Uid', 'Pwd'];
                      const missingKeys = requiredKeys.filter(key => 
                        !value.toLowerCase().includes(key.toLowerCase() + '=')
                      );
                      if (missingKeys.length > 0) {
                        return `MySQL connection string must include: ${missingKeys.join(', ')}`;
                      }
                    } else if (dbType === 'SQLServer') {
                      const requiredKeys = ['Server', 'Database', 'User Id', 'Password'];
                      const missingKeys = requiredKeys.filter(key => 
                        !value.toLowerCase().includes(key.toLowerCase() + '=')
                      );
                      if (missingKeys.length > 0) {
                        return `SQL Server connection string must include: ${missingKeys.join(', ')}`;
                      }
                    } else if (dbType === 'MongoDB') {
                      if (!value.startsWith('mongodb://') && !value.startsWith('mongodb+srv://')) {
                        return 'MongoDB connection string must start with mongodb:// or mongodb+srv://';
                      }
                    }
                    
                    return true;
                  }
                })}
                placeholder={getConnectionStringPlaceholder(dbType)}
                className="zen-input font-mono text-sm"
              />
            ) : (
              <div className="space-y-3">
                {dbType === 'MongoDB' ? (
                  <div>
                    <Label htmlFor="mongoConnectionString" className="text-xs text-muted-foreground">
                      MongoDB Connection String
                    </Label>
                    <Input
                      id="mongoConnectionString"
                      {...register('mongoConnectionString', {
                        required: 'MongoDB connection string is required',
                        validate: (value) => {
                          if (!value) return 'MongoDB connection string is required';
                          if (!value.startsWith('mongodb://') && !value.startsWith('mongodb+srv://')) {
                            return 'MongoDB connection string must start with mongodb:// or mongodb+srv://';
                          }
                          return true;
                        }
                      })}
                      placeholder="mongodb://admin:password@localhost:27017/mydb"
                      className="zen-input font-mono text-sm"
                    />
                    {errors.mongoConnectionString && (
                      <p className="text-sm text-destructive font-light">
                        {errors.mongoConnectionString.message}
                      </p>
                    )}
                  </div>
                ) : (
                  <>
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <div className="flex items-center gap-2">
                          <Label htmlFor="host" className="text-xs text-muted-foreground">
                            {dbType === 'MySQL' || dbType === 'SQLServer' ? 'Server' : 'Host'}
                          </Label>
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            onClick={handleDiscoverServers}
                            disabled={isDiscovering || !dbType}
                            className="h-6 px-2 text-xs"
                          >
                            <Search className="w-3 h-3 mr-1" />
                            {isDiscovering ? 'Keşfediliyor...' : 'Keşfet'}
                          </Button>
                        </div>
                        <Input
                          id="host"
                          {...register('host', { required: 'Host/Server is required' })}
                          placeholder={dbType === 'PostgreSQL' ? 'localhost' : dbType === 'MySQL' ? 'localhost' : dbType === 'SQLServer' ? 'localhost\\SQLEXPRESS' : 'localhost'}
                          className="zen-input text-sm"
                        />
                        {errors.host && (
                          <p className="text-sm text-destructive font-light">
                            {errors.host.message}
                          </p>
                        )}
                        
                        {/* Keşfedilen server listesi */}
                        {showServerList && discoveredServers.length > 0 && (
                          <div className="mt-2 p-2 bg-muted rounded border">
                            <p className="text-xs text-muted-foreground mb-2">Keşfedilen sunucular:</p>
                            <div className="space-y-1">
                              {discoveredServers.map((server, index) => (
                                <button
                                  key={index}
                                  type="button"
                                  onClick={() => handleSelectServer(server)}
                                  className="w-full text-left p-1 text-xs bg-background hover:bg-accent rounded cursor-pointer"
                                >
                                  {server}
                                </button>
                              ))}
                            </div>
                          </div>
                        )}
                        
                        {showServerList && discoveredServers.length === 0 && (
                          <div className="mt-2 p-2 bg-muted rounded border">
                            <p className="text-xs text-muted-foreground">Keşfedilen sunucu bulunamadı.</p>
                            <p className="text-xs text-muted-foreground mt-1">
                              Yerel ağda çalışan {dbType} sunucuları aranıyor...
                            </p>
                          </div>
                        )}
                      </div>
                      <div>
                        <Label htmlFor="port" className="text-xs text-muted-foreground">
                          Port
                        </Label>
                        <Input
                          id="port"
                          {...register('port')}
                          placeholder={dbType === 'PostgreSQL' ? '5432' : dbType === 'MySQL' ? '3306' : dbType === 'SQLServer' ? '1433' : '27017'}
                          className="zen-input text-sm"
                        />
                      </div>
                    </div>
                    <div>
                      <Label htmlFor="database" className="text-xs text-muted-foreground">
                        Database
                      </Label>
                        <Input
                          id="database"
                          {...register('database', { required: 'Database is required' })}
                          placeholder={dbType === 'PostgreSQL' ? 'postgres' : dbType === 'MySQL' ? 'mysql' : dbType === 'SQLServer' ? 'master' : 'mydb'}
                          className="zen-input text-sm"
                        />
                      {errors.database && (
                        <p className="text-sm text-destructive font-light">
                          {errors.database.message}
                        </p>
                      )}
                    </div>
                    <div className="grid grid-cols-2 gap-3">
                      <div>
                        <Label htmlFor="username" className="text-xs text-muted-foreground">
                          {dbType === 'MySQL' ? 'Uid' : dbType === 'SQLServer' ? 'User Id' : 'Username'}
                        </Label>
                        <Input
                          id="username"
                          {...register('username', { required: 'Username is required' })}
                          placeholder={dbType === 'PostgreSQL' ? 'postgres' : dbType === 'MySQL' ? 'root' : dbType === 'SQLServer' ? 'sa' : 'admin'}
                          className="zen-input text-sm"
                        />
                        {errors.username && (
                          <p className="text-sm text-destructive font-light">
                            {errors.username.message}
                          </p>
                        )}
                      </div>
                      <div>
                        <Label htmlFor="password" className="text-xs text-muted-foreground">
                          {dbType === 'MySQL' ? 'Pwd' : 'Password'}
                        </Label>
                        <Input
                          id="password"
                          type="password"
                          {...register('password', { required: 'Password is required' })}
                          placeholder="password"
                          className="zen-input text-sm"
                        />
                        {errors.password && (
                          <p className="text-sm text-destructive font-light">
                            {errors.password.message}
                          </p>
                        )}
                      </div>
                    </div>
                  </>
                )}
              </div>
            )}
            {errors.connectionString && (
              <p className="text-sm text-destructive font-light">
                {errors.connectionString.message}
              </p>
            )}
            {dbType && (
              <div className="text-xs text-muted-foreground space-y-2">
                <div>
                  <p className="font-medium mb-1">Format for {dbType}:</p>
                  <p className="font-mono bg-muted p-2 rounded text-xs">
                    {getConnectionStringFormat(dbType)}
                  </p>
                </div>
                <div>
                  <p className="font-medium mb-1">Example:</p>
                  <p className="font-mono bg-muted p-2 rounded text-xs">
                    {getConnectionStringExample(dbType)}
                  </p>
                </div>
              </div>
            )}
          </div>

          {/* Test Connection Button */}
          <Button
            type="button"
            variant="outline"
            onClick={handleTestConnection}
            disabled={
              !dbType || 
              testMutation.isPending ||
              (inputMode === 'string' && !connectionString) ||
              (inputMode === 'fields' && dbType !== 'MongoDB' && (!host || !database || !username || !password)) ||
              (inputMode === 'fields' && dbType === 'MongoDB' && !mongoConnectionString)
            }
            className="w-full zen-button"
          >
            <TestTube className="w-4 h-4 mr-2" />
            {testMutation.isPending ? 'Testing...' : 'Test Connection'}
          </Button>

          <DialogFooter className="gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                setOpen(false);
                reset();
              }}
              className="zen-button"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={createMutation.isPending}
              className="zen-button"
            >
              {createMutation.isPending ? 'Creating...' : 'Create Connection'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

