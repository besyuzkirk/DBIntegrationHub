import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';

export interface TableInfo {
  name: string;
  schema: string;
  columns: ColumnInfo[];
}

export interface ColumnInfo {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  maxLength?: number;
  numericPrecision?: number;
  numericScale?: number;
}

export interface ConnectionSchemaResponse {
  tables: TableInfo[];
}

export function useConnectionSchema(connectionId?: string) {
  return useQuery({
    queryKey: ['connection-schema', connectionId],
    queryFn: async (): Promise<ConnectionSchemaResponse> => {
      if (!connectionId) {
        throw new Error('Connection ID is required');
      }
      
      const response = await apiClient.get(`/connections/${connectionId}/schema`);
      return response.data;
    },
    enabled: !!connectionId,
  });
}
