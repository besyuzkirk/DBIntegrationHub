'use client';

import { useState } from 'react';
import { Card } from '@/components/ui/card';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '@/components/ui/collapsible';
import { ColumnMapping } from '@/hooks/useMappings';
import { ChevronUp, ChevronDown, ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface MappingPreviewProps {
  mappings: ColumnMapping[];
}

export function MappingPreview({ mappings }: MappingPreviewProps) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <div className="fixed bottom-20 right-6 w-80 z-10">
      <Collapsible open={isOpen} onOpenChange={setIsOpen}>
        <Card className="zen-card">
          <CollapsibleTrigger asChild>
            <Button
              variant="ghost"
              className="w-full flex items-center justify-between p-4 hover:bg-accent/50 rounded-xl"
            >
              <span className="font-light text-sm">
                Current Mappings ({mappings.length})
              </span>
              {isOpen ? (
                <ChevronDown className="w-4 h-4" />
              ) : (
                <ChevronUp className="w-4 h-4" />
              )}
            </Button>
          </CollapsibleTrigger>

          <CollapsibleContent className="px-4 pb-4">
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {mappings.length === 0 ? (
                <p className="text-xs text-muted-foreground font-light text-center py-4">
                  No mappings yet
                </p>
              ) : (
                mappings.map((mapping, index) => (
                  <div
                    key={index}
                    className="flex items-center gap-2 p-2 rounded-lg bg-muted/30 text-xs font-light"
                  >
                    <span className="flex-1 truncate">{mapping.source_column}</span>
                    <ArrowRight className="w-3 h-3 text-primary" />
                    <span className="flex-1 truncate text-right">{mapping.target_parameter}</span>
                  </div>
                ))
              )}
            </div>
          </CollapsibleContent>
        </Card>
      </Collapsible>
    </div>
  );
}

