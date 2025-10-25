'use client';

import { useState } from 'react';
import { ColumnMapping } from '@/hooks/useMappings';
import { ArrowRight, X, GripVertical } from 'lucide-react';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Card } from '@/components/ui/card';

interface MappingCanvasProps {
  mappings: ColumnMapping[];
  onRemoveMapping: (mapping: ColumnMapping) => void;
  onDrop: (sourceColumn: string, targetParameter: string) => void;
}

export function MappingCanvas({ mappings, onRemoveMapping, onDrop }: MappingCanvasProps) {
  const [isDraggingOver, setIsDraggingOver] = useState(false);
  const [dragData, setDragData] = useState<{ source?: string; target?: string }>({});

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDraggingOver(true);
  };

  const handleDragLeave = () => {
    setIsDraggingOver(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDraggingOver(false);

    const sourceColumn = e.dataTransfer.getData('sourceColumn');
    const targetParameter = e.dataTransfer.getData('targetParameter');

    // Eğer hem source hem target varsa mapping oluştur
    if (sourceColumn && targetParameter) {
      onDrop(sourceColumn, targetParameter);
    } else {
      // Sadece source veya target varsa store'da tut
      setDragData((prev) => ({
        source: sourceColumn || prev.source,
        target: targetParameter || prev.target,
      }));

      // Eğer ikisi de doluysa mapping oluştur
      const source = sourceColumn || dragData.source;
      const target = targetParameter || dragData.target;
      
      if (source && target) {
        onDrop(source, target);
        setDragData({});
      }
    }
  };

  return (
    <Card
      className="h-full flex flex-col p-6 border-2 border-dashed transition-all duration-300"
      style={{
        borderColor: isDraggingOver ? 'hsl(var(--primary))' : 'hsl(var(--border))',
        backgroundColor: isDraggingOver ? 'hsl(var(--primary) / 0.05)' : 'transparent',
      }}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
      onDrop={handleDrop}
    >
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between">
          <h3 className="text-xl font-light text-foreground flex items-center gap-2">
            <GripVertical className="w-5 h-5 text-muted-foreground" />
            Current Mappings
          </h3>
          <div className="flex items-center gap-2">
            {dragData.source && (
              <span className="text-xs px-2 py-1 rounded-lg bg-primary/10 text-primary">
                Source: {dragData.source}
              </span>
            )}
            {dragData.target && (
              <span className="text-xs px-2 py-1 rounded-lg bg-primary/10 text-primary">
                Target: {dragData.target}
              </span>
            )}
            <span className="text-sm font-normal text-muted-foreground">
              ({mappings.length} {mappings.length === 1 ? 'mapping' : 'mappings'})
            </span>
          </div>
        </div>
        <p className="text-xs text-muted-foreground font-light mt-2">
          {isDraggingOver 
            ? '🎯 Drop here to create mapping' 
            : 'Drag source and target columns here, or click items from both sides'}
        </p>
      </div>

      {/* Mappings List */}
      <ScrollArea className="flex-1">
        {mappings.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-full min-h-[400px] text-center">
            <div className="w-20 h-20 rounded-full bg-muted/30 flex items-center justify-center mb-4">
              <ArrowRight className="w-10 h-10 text-muted-foreground/50" />
            </div>
            <p className="text-muted-foreground font-light text-lg mb-2">
              No mappings yet
            </p>
            <p className="text-sm text-muted-foreground/70 font-light max-w-md">
              Drag a source column from the left panel and drop it here,<br />
              then drag a target parameter from the right panel
            </p>
          </div>
        ) : (
          <div className="space-y-3 pr-4">
            {mappings.map((mapping, index) => (
              <div
                key={index}
                className="group relative"
              >
                {/* Mapping Card */}
                <div className="flex items-center gap-4 p-4 rounded-2xl bg-gradient-to-r from-primary/5 via-transparent to-primary/5 border border-border/50 hover:border-primary/30 hover:shadow-md transition-all duration-400">
                  {/* Source Column */}
                  <div className="flex-1 flex items-center gap-3 min-w-0">
                    <div className="w-2 h-2 rounded-full bg-primary animate-pulse" />
                    <div>
                      <p className="text-xs text-muted-foreground font-light">Source</p>
                      <p className="text-sm font-normal text-foreground truncate">
                        {mapping.sourceColumn}
                      </p>
                    </div>
                  </div>

                  {/* Arrow */}
                  <div className="flex-shrink-0">
                    <div className="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center">
                      <ArrowRight className="w-5 h-5 text-primary" />
                    </div>
                  </div>

                  {/* Target Parameter */}
                  <div className="flex-1 flex items-center gap-3 min-w-0 justify-end text-right">
                    <div>
                      <p className="text-xs text-muted-foreground font-light">Target</p>
                      <p className="text-sm font-normal text-foreground truncate">
                        {mapping.targetParameter}
                      </p>
                    </div>
                    <div className="w-2 h-2 rounded-full bg-primary animate-pulse" />
                  </div>

                  {/* Delete Button */}
                  <button
                    onClick={() => onRemoveMapping(mapping)}
                    className="absolute -top-2 -right-2 w-7 h-7 rounded-full bg-destructive/10 hover:bg-destructive hover:text-white text-destructive opacity-0 group-hover:opacity-100 transition-all duration-300 flex items-center justify-center shadow-sm hover:shadow-md hover:scale-110"
                    title="Remove mapping"
                  >
                    <X className="w-4 h-4" />
                  </button>
                </div>

                {/* Index Badge */}
                <div className="absolute -left-3 top-1/2 -translate-y-1/2 w-6 h-6 rounded-full bg-muted border border-border flex items-center justify-center">
                  <span className="text-xs font-normal text-muted-foreground">
                    {index + 1}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </ScrollArea>
    </Card>
  );
}

