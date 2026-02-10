"use client";

import { useState, useEffect } from "react";
import { formatDate } from "@/lib/utils";
import { useApi } from "@/hooks/useApi";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";

interface VersionHistoryItem {
  version: number;
  createdAt: string;
  updatedAt: string;
  createdBy?: string | null;
  updatedBy?: string | null;
  isDeleted: boolean;
}

interface VersionHistoryDialogProps {
  name: string;
  entityType: "template" | "question" | "option";
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function VersionHistoryDialog({
  name,
  entityType,
  open,
  onOpenChange,
}: VersionHistoryDialogProps) {
  const api = useApi();
  const [loading, setLoading] = useState(false);
  const [history, setHistory] = useState<VersionHistoryItem[]>([]);

  useEffect(() => {
    if (!open || !name) {
      setHistory([]);
      return;
    }

    setLoading(true);
    api.get<VersionHistoryItem[]>(
      `/QuestionnaireTemplate/${encodeURIComponent(name)}/history/${entityType}`
    )
      .then((response) => {
        const data = response?.data || response;
        setHistory(Array.isArray(data) ? data : []);
      })
      .catch((error) => {
        console.error("Failed to load version history", error);
        setHistory([]);
      })
      .finally(() => {
        setLoading(false);
      });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, name, entityType]);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Version History</DialogTitle>
          <DialogDescription>
            View all versions of {entityType} "{name}"
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-3 mt-4">
          {loading ? (
            <div className="text-sm text-muted-foreground text-center py-8">
              Loading version history...
            </div>
          ) : history.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-8">
              No version history found
            </p>
          ) : (
            history.map((item) => (
              <div
                key={item.version}
                className="p-4 border rounded-lg space-y-2 hover:bg-muted/50 transition-colors"
              >
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">v{item.version}</Badge>
                    {item.isDeleted && (
                      <Badge variant="destructive" className="text-xs">
                        Deleted
                      </Badge>
                    )}
                  </div>
                  <div className="text-xs text-muted-foreground">
                    {formatDate(item.createdAt)}
                  </div>
                </div>
                <div className="text-sm space-y-1">
                  {item.createdBy && (
                    <div>
                      <span className="text-muted-foreground">Created by: </span>
                      <span>{item.createdBy}</span>
                    </div>
                  )}
                  {item.updatedBy && item.updatedBy !== item.createdBy && (
                    <div>
                      <span className="text-muted-foreground">Updated by: </span>
                      <span>{item.updatedBy}</span>
                    </div>
                  )}
                  {item.updatedAt !== item.createdAt && (
                    <div className="text-xs text-muted-foreground">
                      Last updated: {formatDate(item.updatedAt)}
                    </div>
                  )}
                </div>
              </div>
            ))
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
