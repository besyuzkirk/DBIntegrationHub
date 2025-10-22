'use client';

import { Button } from '@/components/ui/button';
import { Plus, Save, Loader2 } from 'lucide-react';

interface MappingToolbarProps {
  onAddMapping: () => void;
  onSave: () => void;
  canAddMapping: boolean;
  isSaving: boolean;
}

export function MappingToolbar({
  onAddMapping,
  onSave,
  canAddMapping,
  isSaving,
}: MappingToolbarProps) {
  return (
    <div className="fixed bottom-0 left-0 right-0 lg:left-64 border-t border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="zen-container py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Button
              onClick={onAddMapping}
              disabled={!canAddMapping}
              className="zen-button"
            >
              <Plus className="w-4 h-4 mr-2" />
              Add Mapping
            </Button>
            <p className="text-sm text-muted-foreground font-light">
              Select one source and one target
            </p>
          </div>

          <Button
            onClick={onSave}
            disabled={isSaving}
            className="zen-button bg-primary text-primary-foreground"
          >
            {isSaving ? (
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
            ) : (
              <Save className="w-4 h-4 mr-2" />
            )}
            {isSaving ? 'Saving...' : 'Save Mappings'}
          </Button>
        </div>
      </div>
    </div>
  );
}

