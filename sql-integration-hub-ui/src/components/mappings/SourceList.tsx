'use client';

import { useState } from 'react';
import { Input } from '@/components/ui/input';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Search, GripVertical } from 'lucide-react';
import { cn } from '@/lib/utils';

interface SourceListProps {
  columns: string[];
  selectedColumn: string | null;
  onSelectColumn: (column: string) => void;
}

export function SourceList({ columns, selectedColumn, onSelectColumn }: SourceListProps) {
  const [search, setSearch] = useState('');
  const [draggedColumn, setDraggedColumn] = useState<string | null>(null);

  const filteredColumns = columns.filter((col) =>
    col.toLowerCase().includes(search.toLowerCase())
  );

  const handleDragStart = (e: React.DragEvent, column: string) => {
    e.dataTransfer.setData('sourceColumn', column);
    e.dataTransfer.effectAllowed = 'copy';
    setDraggedColumn(column);
  };

  const handleDragEnd = () => {
    setDraggedColumn(null);
  };

  return (
    <div className="h-full flex flex-col bg-card rounded-2xl border border-border shadow-sm p-6">
      <div className="mb-4">
        <h3 className="text-lg font-light text-foreground mb-4">Source Columns</h3>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            placeholder="Search columns..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9 zen-input"
          />
        </div>
      </div>

      <ScrollArea className="flex-1 pr-4">
        <div className="space-y-2">
          {filteredColumns.length === 0 ? (
            <p className="text-sm text-muted-foreground font-light text-center py-8">
              No columns found
            </p>
          ) : (
            filteredColumns.map((column) => (
              <div
                key={column}
                draggable
                onDragStart={(e) => handleDragStart(e, column)}
                onDragEnd={handleDragEnd}
                onClick={() => onSelectColumn(column)}
                className={cn(
                  'w-full flex items-center gap-3 px-4 py-3 rounded-xl transition-all duration-300 cursor-grab active:cursor-grabbing',
                  'hover:bg-accent/50 hover:translate-x-1',
                  draggedColumn === column && 'opacity-50 scale-95',
                  selectedColumn === column
                    ? 'bg-primary/10 border-2 border-primary text-primary font-normal'
                    : 'bg-muted/30 border-2 border-transparent text-foreground font-light'
                )}
              >
                <GripVertical className="w-4 h-4 text-muted-foreground/50 flex-shrink-0" />
                <div className="flex items-center gap-2 flex-1 min-w-0">
                  <div className="w-2 h-2 rounded-full bg-current flex-shrink-0" />
                  <span className="text-sm truncate">{column}</span>
                </div>
              </div>
            ))
          )}
        </div>
      </ScrollArea>

      <div className="mt-4 pt-4 border-t border-border">
        <p className="text-xs text-muted-foreground font-light">
          🎯 {filteredColumns.length} column{filteredColumns.length !== 1 ? 's' : ''} • Drag to center
        </p>
      </div>
    </div>
  );
}

