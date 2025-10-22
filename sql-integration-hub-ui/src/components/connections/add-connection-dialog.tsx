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
import { Plus, TestTube } from 'lucide-react';

interface FormData {
  name: string;
  databaseType: 'PostgreSQL' | 'MySQL' | 'SQLServer' | 'MongoDB';
  connectionString: string;
}

export function AddConnectionDialog() {
  const [open, setOpen] = useState(false);
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

  const onSubmit = async (data: FormData) => {
    await createMutation.mutateAsync(data);
    setOpen(false);
    reset();
  };

  const handleTestConnection = async () => {
    if (!connectionString || !dbType) {
      return;
    }
    await testMutation.mutateAsync({
      connectionString: connectionString,
      databaseType: dbType,
    });
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
              onValueChange={(value) =>
                setValue('databaseType', value as 'PostgreSQL' | 'MySQL' | 'SQLServer' | 'MongoDB')
              }
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
            <Label htmlFor="connectionString" className="text-sm font-normal">
              Connection String
            </Label>
            <Input
              id="connectionString"
              {...register('connectionString', {
                required: 'Connection string is required',
              })}
              placeholder="Host=localhost;Database=mydb;Username=user;Password=***"
              className="zen-input font-mono text-sm"
            />
            {errors.connectionString && (
              <p className="text-sm text-destructive font-light">
                {errors.connectionString.message}
              </p>
            )}
          </div>

          {/* Test Connection Button */}
          <Button
            type="button"
            variant="outline"
            onClick={handleTestConnection}
            disabled={!connectionString || !dbType || testMutation.isPending}
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

