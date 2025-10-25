import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/axios';

export interface ColumnInfo {
  name: string;
  dataType: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  maxLength: number | null;
  precision: number | null;
  scale: number | null;
}

export interface TableColumnsResponse {
  columns: ColumnInfo[];
}

export function useTableColumns(
  connectionId?: string, 
  tableName?: string, 
  schema: string = 'dbo'
) {
  return useQuery<TableColumnsResponse>({
    queryKey: ['tableColumns', connectionId, tableName, schema],
    queryFn: async () => {
      if (!connectionId || !tableName) {
        return { columns: [] };
      }
      
      const response = await apiClient.get(
        `/connections/${connectionId}/schema/${tableName}/columns?schema=${schema}`
      );
      return response.data;
    },
    enabled: !!connectionId && !!tableName,
    staleTime: 5 * 60 * 1000, // 5 dakika cache
  });
}
