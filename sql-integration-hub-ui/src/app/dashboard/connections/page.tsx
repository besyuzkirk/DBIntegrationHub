'use client';

import { useState } from 'react';
import { useConnections, useDeleteConnection } from '@/hooks/useConnections';
import { AddConnectionDialog } from '@/components/connections/add-connection-dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
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
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Trash2, Loader2, Database } from 'lucide-react';
import { format } from 'date-fns';

export default function ConnectionsPage() {
  const { data: connections, isLoading, error } = useConnections();
  const deleteMutation = useDeleteConnection();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const handleDelete = async () => {
    if (deleteId) {
      await deleteMutation.mutateAsync(deleteId);
      setDeleteId(null);
    }
  };

  const getDbTypeColor = (dbType: string) => {
    switch (dbType) {
      case 'SQLServer':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200';
      case 'PostgreSQL':
        return 'bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-200';
      case 'MySQL':
        return 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200';
      case 'MongoDB':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-200';
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-4xl font-light text-foreground mb-2">
            Connections
          </h1>
          <p className="text-muted-foreground font-light">
            Manage your database connections
          </p>
        </div>
        <AddConnectionDialog />
      </div>

      {/* Connections Table */}
      <Card className="zen-card">
        <CardHeader>
          <CardTitle className="text-2xl font-light flex items-center gap-2">
            <Database className="w-5 h-5" />
            Database Connections
          </CardTitle>
          <CardDescription className="font-light">
            All your saved database connections
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="w-8 h-8 animate-spin text-muted-foreground" />
            </div>
          ) : error ? (
            <div className="text-center py-12">
              <p className="text-destructive font-light">
                Failed to load connections. Please try again.
              </p>
            </div>
          ) : !connections || connections.length === 0 ? (
            <div className="text-center py-12">
              <Database className="w-12 h-12 mx-auto mb-4 text-muted-foreground opacity-50" />
              <p className="text-muted-foreground font-light text-lg mb-2">
                No connections yet
              </p>
              <p className="text-sm text-muted-foreground font-light">
                Click "Add Connection" to create your first database connection
              </p>
            </div>
          ) : (
            <div className="rounded-xl border border-border overflow-hidden">
              <Table>
                <TableHeader>
                  <TableRow className="bg-muted/50">
                    <TableHead className="font-normal">Name</TableHead>
                    <TableHead className="font-normal">Database Type</TableHead>
                    <TableHead className="font-normal">Connection String</TableHead>
                    <TableHead className="font-normal">Created At</TableHead>
                    <TableHead className="font-normal text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {connections.map((connection) => (
                    <TableRow
                      key={connection.id}
                      className="transition-colors duration-300 hover:bg-muted/30"
                    >
                      <TableCell className="font-normal">
                        {connection.name}
                      </TableCell>
                      <TableCell>
                        <Badge
                          className={`${getDbTypeColor(connection.databaseType)} font-light`}
                          variant="secondary"
                        >
                          {connection.databaseType}
                        </Badge>
                      </TableCell>
                      <TableCell className="font-mono text-xs text-muted-foreground max-w-xs truncate">
                        {connection.connectionString}
                      </TableCell>
                      <TableCell className="text-muted-foreground font-light">
                        {format(new Date(connection.createdAt), 'PPp')}
                      </TableCell>
                      <TableCell className="text-right">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDeleteId(connection.id)}
                          className="hover:bg-destructive/10 hover:text-destructive transition-all duration-300"
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteId !== null} onOpenChange={() => setDeleteId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle className="text-2xl font-light">
              Delete Connection
            </AlertDialogTitle>
            <AlertDialogDescription className="font-light">
              Are you sure you want to delete this connection? This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel className="zen-button">Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDelete}
              className="zen-button bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
