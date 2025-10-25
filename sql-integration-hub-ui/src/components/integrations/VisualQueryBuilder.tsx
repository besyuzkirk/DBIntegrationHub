'use client';

import { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { 
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { 
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command';
import { 
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { 
  Database, 
  Table as TableIcon, 
  Plus, 
  X,
  Search,
  Filter,
  ArrowUpDown,
  Hash,
  ChevronDown,
  ChevronsUpDown,
  Check,
  Eye,
  EyeOff,
  Trash2,
  Settings
} from 'lucide-react';
import { useConnectionSchema, TableInfo, ColumnInfo } from '@/hooks/useConnectionSchema';
import { useTableColumns } from '@/hooks/useTableColumns';

// Debounced search hook
function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

interface VisualQueryBuilderProps {
  connectionId?: string;
  onQueryChange: (query: string) => void;
  initialQuery?: string;
}

interface SelectedTable {
  table: TableInfo;
  selectedColumns: string[];
  alias: string;
}

interface WhereCondition {
  column: string;
  operator: string;
  value: string;
}

interface OrderByCondition {
  column: string;
  direction: 'ASC' | 'DESC';
}

export function VisualQueryBuilder({ 
  connectionId, 
  onQueryChange, 
  initialQuery = '' 
}: VisualQueryBuilderProps) {
  const { data: schema, isLoading } = useConnectionSchema(connectionId);
  const [selectedTables, setSelectedTables] = useState<SelectedTable[]>([]);
  const [whereConditions, setWhereConditions] = useState<WhereCondition[]>([]);
  const [orderByConditions, setOrderByConditions] = useState<OrderByCondition[]>([]);
  const [limit, setLimit] = useState<number | null>(null);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [isTableSelectorOpen, setIsTableSelectorOpen] = useState(false);
  
  // Debounced search - 150ms gecikme ile (daha hızlı)
  const debouncedSearchQuery = useDebounce(searchQuery, 150);

  // Filtrelenmiş tabloları memoize et - debounced query kullan
  const filteredTables = useMemo(() => {
    if (!schema?.tables) return [];
    
    if (!debouncedSearchQuery.trim()) {
      // Arama yoksa tüm tabloları göster
      return schema.tables;
    }
    
    const query = debouncedSearchQuery.toLowerCase();
    const filtered = [];
    
    // Tüm tablolarda ara ama performans için optimize et
    for (let i = 0; i < schema.tables.length; i++) {
      const table = schema.tables[i];
      if (table.name.toLowerCase().includes(query) || 
          table.schema.toLowerCase().includes(query)) {
        filtered.push(table);
      }
    }
    
    return filtered;
  }, [schema?.tables, debouncedSearchQuery]);

  // SQL sorgusunu oluştur
  const generateSQL = useCallback(() => {
    if (selectedTables.length === 0) return '';

    const selectColumns = selectedTables.flatMap(st => 
      st.selectedColumns.map(col => 
        st.alias ? `${st.alias}.${col}` : `${st.table.name}.${col}`
      )
    );

    if (selectColumns.length === 0) return '';

    const fromClause = selectedTables.map(st => 
      st.alias ? `${st.table.name} AS ${st.alias}` : st.table.name
    ).join(', ');

    const whereClause = whereConditions.length > 0 
      ? `WHERE ${whereConditions.map(wc => {
          if (wc.operator === 'IS NULL' || wc.operator === 'IS NOT NULL') {
            return `${wc.column} ${wc.operator}`;
          } else if (wc.operator === 'IN' || wc.operator === 'NOT IN') {
            // IN operatörü için değerleri parantez içinde göster
            const values = wc.value.split(',').map(v => `'${v.trim()}'`).join(', ');
            return `${wc.column} ${wc.operator} (${values})`;
          } else {
            return `${wc.column} ${wc.operator} '${wc.value}'`;
          }
        }).join(' AND ')}`
      : '';

    const orderByClause = orderByConditions.length > 0 
      ? `ORDER BY ${orderByConditions.map(obc => `${obc.column} ${obc.direction}`).join(', ')}`
      : '';

    const limitClause = limit ? `LIMIT ${limit}` : '';

    return `SELECT ${selectColumns.join(', ')}\nFROM ${fromClause}\n${whereClause}\n${orderByClause}\n${limitClause}`.trim();
  }, [selectedTables, whereConditions, orderByConditions, limit]);

  // SQL sorgusunu güncelle
  useEffect(() => {
    const sql = generateSQL();
    onQueryChange(sql);
  }, [generateSQL]);

  const addTable = useCallback((table: TableInfo) => {
    setSelectedTables(prev => {
      if (prev.some(st => st.table.name === table.name)) return prev;
      
      return [...prev, {
        table,
        selectedColumns: [],
        alias: ''
      }];
    });
    setIsTableSelectorOpen(false);
    setSearchQuery('');
  }, []);

  const removeTable = useCallback((tableName: string) => {
    setSelectedTables(prev => prev.filter(st => st.table.name !== tableName));
  }, []);

  const toggleColumn = useCallback((tableName: string, columnName: string) => {
    setSelectedTables(prev => prev.map(st => 
      st.table.name === tableName 
        ? { 
            ...st, 
            selectedColumns: st.selectedColumns.includes(columnName)
              ? st.selectedColumns.filter(col => col !== columnName)
              : [...st.selectedColumns, columnName]
          }
        : st
    ));
  }, []);

  const addWhereCondition = useCallback(() => {
    setWhereConditions(prev => [...prev, { column: '', operator: '=', value: '' }]);
  }, []);

  const removeWhereCondition = useCallback((index: number) => {
    setWhereConditions(prev => prev.filter((_, i) => i !== index));
  }, []);

  const updateWhereCondition = useCallback((index: number, field: keyof WhereCondition, value: string) => {
    setWhereConditions(prev => prev.map((wc, i) => 
      i === index ? { ...wc, [field]: value } : wc
    ));
  }, []);

  const addOrderByCondition = useCallback(() => {
    setOrderByConditions(prev => [...prev, { column: '', direction: 'ASC' }]);
  }, []);

  const removeOrderByCondition = useCallback((index: number) => {
    setOrderByConditions(prev => prev.filter((_, i) => i !== index));
  }, []);

  const updateOrderByCondition = useCallback((index: number, field: keyof OrderByCondition, value: string) => {
    setOrderByConditions(prev => prev.map((obc, i) => 
      i === index ? { ...obc, [field]: value } : obc
    ));
  }, []);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-center">
          <Database className="w-12 h-12 mx-auto mb-4 animate-pulse text-muted-foreground" />
          <p className="text-muted-foreground">Schema bilgileri yükleniyor...</p>
        </div>
      </div>
    );
  }

  if (!schema?.tables || schema.tables.length === 0) {
    return (
      <div className="text-center py-8">
        <Database className="w-16 h-16 mx-auto mb-4 text-muted-foreground" />
        <h3 className="text-lg font-semibold mb-2">Tablo bulunamadı</h3>
        <p className="text-muted-foreground">Bu bağlantı için tablo bilgileri alınamadı.</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Visual Query Builder</h2>
          <p className="text-muted-foreground">Görsel olarak SQL sorgusu oluşturun</p>
        </div>
        <Badge variant="outline" className="flex items-center gap-2">
          <Database className="w-4 h-4" />
          {schema.tables.length} tablo
        </Badge>
      </div>

      {/* Table Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TableIcon className="w-5 h-5" />
            Tablolar
          </CardTitle>
          <CardDescription>
            Sorguya dahil etmek istediğiniz tabloları seçin
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {/* Table Search */}
            <Popover open={isTableSelectorOpen} onOpenChange={setIsTableSelectorOpen}>
              <PopoverTrigger asChild>
                <Button
                  variant="outline"
                  className="w-full justify-between h-12"
                >
                  <div className="flex items-center gap-2">
                    <Search className="w-4 h-4" />
                    <span>Tablo ara ve seç...</span>
                  </div>
                  <ChevronDown className="w-4 h-4" />
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-full p-0" align="start">
                <Command>
                  <CommandInput 
                    placeholder="Tablo ara..." 
                    value={searchQuery}
                    onValueChange={(value: string) => setSearchQuery(value)}
                  />
                  <CommandList className="max-h-80 overflow-y-auto overflow-x-hidden">
                    <CommandEmpty>Tablo bulunamadı.</CommandEmpty>
                    <CommandGroup className="max-h-80 overflow-y-auto">
                      {filteredTables.map((table: TableInfo) => (
                        <CommandItem
                          key={table.name}
                          value={`${table.name} ${table.schema}`}
                          onSelect={() => addTable(table)}
                          className="flex items-center space-x-2"
                        >
                          <Check
                            className={`mr-2 h-4 w-4 ${
                              selectedTables.some(st => st.table.name === table.name)
                                ? 'opacity-100'
                                : 'opacity-0'
                            }`}
                          />
                          <div className="flex flex-col min-w-0 flex-1">
                            <span className="font-medium truncate">{table.name}</span>
                            <span className="text-sm text-muted-foreground truncate">
                              {table.schema} • {table.columns.length} kolon
                            </span>
                          </div>
                        </CommandItem>
                      ))}
                    </CommandGroup>
                  </CommandList>
                </Command>
              </PopoverContent>
            </Popover>

            {/* Selected Tables */}
            {selectedTables.length > 0 && (
              <div className="space-y-2">
                <Label>Seçilen Tablolar:</Label>
                <div className="flex flex-wrap gap-2">
                  {selectedTables.map((selectedTable) => (
                    <Badge
                      key={selectedTable.table.name}
                      variant="secondary"
                      className="flex items-center gap-2 px-3 py-1"
                    >
                      <TableIcon className="w-3 h-3" />
                      {selectedTable.table.name}
                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-auto p-0 ml-1 hover:bg-destructive hover:text-destructive-foreground"
                        onClick={() => removeTable(selectedTable.table.name)}
                      >
                        <X className="w-3 h-3" />
                      </Button>
                    </Badge>
                  ))}
                </div>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Selected Tables and Columns */}
      {selectedTables.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Settings className="w-5 h-5" />
              Kolon Seçimi
            </CardTitle>
            <CardDescription>
              Her tablo için istediğiniz kolonları seçin
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-6">
            {selectedTables.map((selectedTable) => (
              <TableColumns 
                key={selectedTable.table.name}
                table={selectedTable.table}
                connectionId={connectionId}
                selectedColumns={selectedTable.selectedColumns}
                onToggleColumn={(columnName) => toggleColumn(selectedTable.table.name, columnName)}
              />
            ))}
          </CardContent>
        </Card>
      )}

      {/* WHERE Conditions */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="w-5 h-5" />
            WHERE Koşulları
          </CardTitle>
          <CardDescription>
            Veri filtreleme koşulları ekleyin
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {whereConditions.map((condition, index) => (
            <div key={index} className="flex items-center gap-2 p-4 border rounded-lg">
              <Select
                value={condition.column}
                onValueChange={(value: string) => updateWhereCondition(index, 'column', value)}
              >
                <SelectTrigger className="w-48">
                  <SelectValue placeholder="Kolon seçin" />
                </SelectTrigger>
                <SelectContent className="max-h-60 overflow-y-auto">
                  {selectedTables.flatMap(st => 
                    st.selectedColumns.map(col => (
                      <SelectItem key={`${st.table.name}.${col}`} value={`${st.table.name}.${col}`}>
                        {st.table.name}.{col}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
              
              <Select
                value={condition.operator}
                onValueChange={(value: string) => updateWhereCondition(index, 'operator', value)}
              >
                <SelectTrigger className="w-32">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="=">Eşittir (=)</SelectItem>
                  <SelectItem value="!=">Eşit değildir (!=)</SelectItem>
                  <SelectItem value="&gt;">Büyüktür (&gt;)</SelectItem>
                  <SelectItem value="&lt;">Küçüktür (&lt;)</SelectItem>
                  <SelectItem value="&gt;=">Büyük eşittir (&gt;=)</SelectItem>
                  <SelectItem value="&lt;=">Küçük eşittir (&lt;=)</SelectItem>
                  <SelectItem value="LIKE">İçerir (LIKE)</SelectItem>
                  <SelectItem value="IN">İçinde (IN)</SelectItem>
                  <SelectItem value="NOT IN">İçinde değil (NOT IN)</SelectItem>
                  <SelectItem value="IS NULL">Boş (IS NULL)</SelectItem>
                  <SelectItem value="IS NOT NULL">Boş değil (IS NOT NULL)</SelectItem>
                </SelectContent>
              </Select>
              
              <Input
                placeholder={
                  condition.operator === 'IS NULL' || condition.operator === 'IS NOT NULL' 
                    ? 'Değer gerekmez' 
                    : condition.operator === 'IN' || condition.operator === 'NOT IN'
                    ? 'Değer1, Değer2, Değer3'
                    : 'Değer'
                }
                value={condition.value}
                onChange={(e) => updateWhereCondition(index, 'value', e.target.value)}
                className="flex-1"
                disabled={condition.operator === 'IS NULL' || condition.operator === 'IS NOT NULL'}
              />
              
              <Button
                variant="ghost"
                size="sm"
                onClick={() => removeWhereCondition(index)}
                className="text-destructive hover:text-destructive"
              >
                <Trash2 className="w-4 h-4" />
              </Button>
            </div>
          ))}
          
          <Button
            variant="outline"
            onClick={addWhereCondition}
            className="w-full"
          >
            <Plus className="w-4 h-4 mr-2" />
            WHERE Koşulu Ekle
          </Button>
        </CardContent>
      </Card>

      {/* ORDER BY */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <ArrowUpDown className="w-5 h-5" />
            ORDER BY
          </CardTitle>
          <CardDescription>
            Sonuçları sıralayın
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {orderByConditions.map((condition, index) => (
            <div key={index} className="flex items-center gap-2 p-4 border rounded-lg">
              <Select
                value={condition.column}
                onValueChange={(value: string) => updateOrderByCondition(index, 'column', value)}
              >
                <SelectTrigger className="w-48">
                  <SelectValue placeholder="Kolon seçin" />
                </SelectTrigger>
                <SelectContent className="max-h-60 overflow-y-auto">
                  {selectedTables.flatMap(st => 
                    st.selectedColumns.map(col => (
                      <SelectItem key={`${st.table.name}.${col}`} value={`${st.table.name}.${col}`}>
                        {st.table.name}.{col}
                      </SelectItem>
                    ))
                  )}
                </SelectContent>
              </Select>
              
              <Select
                value={condition.direction}
                onValueChange={(value: string) => updateOrderByCondition(index, 'direction', value as 'ASC' | 'DESC')}
              >
                <SelectTrigger className="w-32">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="ASC">Artan (ASC)</SelectItem>
                  <SelectItem value="DESC">Azalan (DESC)</SelectItem>
                </SelectContent>
              </Select>
              
              <Button
                variant="ghost"
                size="sm"
                onClick={() => removeOrderByCondition(index)}
                className="text-destructive hover:text-destructive"
              >
                <Trash2 className="w-4 h-4" />
              </Button>
            </div>
          ))}
          
          <Button
            variant="outline"
            onClick={addOrderByCondition}
            className="w-full"
          >
            <Plus className="w-4 h-4 mr-2" />
            Sıralama Ekle
          </Button>
        </CardContent>
      </Card>

      {/* LIMIT */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Hash className="w-5 h-5" />
            LIMIT
          </CardTitle>
          <CardDescription>
            Sonuç sayısını sınırlayın
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2">
            <Input
              type="number"
              placeholder="Sonuç sayısı"
              value={limit || ''}
              onChange={(e) => setLimit(e.target.value ? parseInt(e.target.value) : null)}
              className="w-32"
            />
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setLimit(null)}
              className="text-muted-foreground"
            >
              <X className="w-4 h-4" />
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

// TableColumns component for lazy loading
const TableColumns = ({ 
  table, 
  connectionId, 
  selectedColumns, 
  onToggleColumn 
}: { 
  table: TableInfo; 
  connectionId?: string; 
  selectedColumns: string[]; 
  onToggleColumn: (columnName: string) => void; 
}) => {
  const { data: columnsData, isLoading } = useTableColumns(
    connectionId, 
    table.name, 
    table.schema
  );

  const columns = columnsData?.columns || table.columns;
  const [isColumnSelectorOpen, setIsColumnSelectorOpen] = useState(false);
  const [columnSearchQuery, setColumnSearchQuery] = useState<string>('');
  
  // Debounced search - 200ms gecikme ile (kolonlar daha az olduğu için daha hızlı)
  const debouncedColumnSearchQuery = useDebounce(columnSearchQuery, 200);

  // Filtrelenmiş kolonları memoize et - debounced query kullan
  const filteredColumns = useMemo(() => {
    if (!debouncedColumnSearchQuery.trim()) return columns;
    
    const query = debouncedColumnSearchQuery.toLowerCase();
    return columns.filter((column: ColumnInfo) => 
      column.name.toLowerCase().includes(query) || 
      column.dataType.toLowerCase().includes(query)
    );
  }, [columns, debouncedColumnSearchQuery]);

  if (isLoading) {
    return (
      <div className="space-y-2">
        <div className="flex items-center gap-2">
          <TableIcon className="w-4 h-4" />
          <span className="font-medium">{table.name}</span>
          <Badge variant="outline" className="text-xs">Yükleniyor...</Badge>
        </div>
        <div className="h-10 bg-muted animate-pulse rounded" />
      </div>
    );
  }

  if (columns.length === 0) {
    return (
      <div className="space-y-2">
        <div className="flex items-center gap-2">
          <TableIcon className="w-4 h-4" />
          <span className="font-medium">{table.name}</span>
          <Badge variant="destructive" className="text-xs">Kolon yok</Badge>
        </div>
        <div className="text-center py-4 text-muted-foreground">
          <Database className="w-8 h-8 mx-auto mb-2 opacity-50" />
          <p className="text-sm">Bu tablo için kolon bilgileri bulunamadı</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2">
        <TableIcon className="w-4 h-4" />
        <span className="font-medium">{table.name}</span>
        <Badge variant="outline" className="text-xs">{columns.length} kolon</Badge>
      </div>

      {/* Kolon Dropdown */}
      <Popover open={isColumnSelectorOpen} onOpenChange={setIsColumnSelectorOpen}>
        <PopoverTrigger asChild>
          <Button 
            variant="outline" 
            role="combobox"
            className="w-full justify-between"
          >
            <span>
              {selectedColumns.length === 0 
                ? "Kolon seçin..." 
                : `${selectedColumns.length} kolon seçildi`
              }
            </span>
            <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-full p-0" align="start">
          <Command>
            <CommandInput 
              placeholder="Kolon ara..." 
              value={columnSearchQuery}
              onValueChange={(value: string) => setColumnSearchQuery(value)}
            />
            <CommandEmpty>Kolon bulunamadı.</CommandEmpty>
            <CommandList className="max-h-64 overflow-y-auto overflow-x-hidden">
              <CommandGroup className="max-h-64 overflow-y-auto">
                {filteredColumns.map((column: ColumnInfo) => (
                  <CommandItem
                    key={column.name}
                    value={column.name}
                    onSelect={() => onToggleColumn(column.name)}
                    className="flex items-center space-x-2"
                  >
                    <Checkbox
                      checked={selectedColumns.includes(column.name)}
                      onChange={() => {}} // CommandItem onSelect ile kontrol ediliyor
                      className="pointer-events-none"
                    />
                    <div className="flex-1">
                      <div className="flex items-center gap-2">
                        <span className="font-medium">{column.name}</span>
                        {column.isPrimaryKey && (
                          <Badge variant="destructive" className="text-xs">PK</Badge>
                        )}
                        {!column.isNullable && (
                          <Badge variant="secondary" className="text-xs">NOT NULL</Badge>
                        )}
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {column.dataType}
                      </div>
                    </div>
                  </CommandItem>
                ))}
              </CommandGroup>
            </CommandList>
          </Command>
        </PopoverContent>
      </Popover>

      {/* Seçilen Kolonlar */}
      {selectedColumns.length > 0 && (
        <div className="space-y-2">
          <p className="text-sm font-medium text-muted-foreground">Seçilen Kolonlar:</p>
          <div className="flex flex-wrap gap-2">
            {selectedColumns.map((columnName: string) => {
              const column = columns.find((col: ColumnInfo) => col.name === columnName);
              return (
                <div key={columnName} className="flex items-center gap-1 px-2 py-1 bg-primary/10 rounded-md">
                  <span className="text-sm">{columnName}</span>
                  {column?.isPrimaryKey && (
                    <Badge variant="destructive" className="text-xs">PK</Badge>
                  )}
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-5 w-5 p-0 hover:bg-destructive hover:text-destructive-foreground"
                    onClick={() => onToggleColumn(columnName)}
                  >
                    <X className="h-3 w-3" />
                  </Button>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};