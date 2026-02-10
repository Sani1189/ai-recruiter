"use client";

import { useState, useEffect } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { 
  Plus, 
  Edit2, 
  Trash2, 
  GripVertical,
  Eye,
  EyeOff
} from "lucide-react";
import { useApi } from "@/hooks/useApi";
import { KanbanBoardColumn } from "@/lib/api/services/jobs.service";

interface KanbanBoardColumnsManagerProps {
  recruiterId: string;
  onColumnsUpdate?: () => void;
}

export default function KanbanBoardColumnsManager({
  recruiterId,
  onColumnsUpdate,
}: KanbanBoardColumnsManagerProps) {
  const [columns, setColumns] = useState<KanbanBoardColumn[]>([]);
  const [loading, setLoading] = useState(false);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingColumn, setEditingColumn] = useState<KanbanBoardColumn | null>(null);
  const [formData, setFormData] = useState({ columnName: "" });
  const api = useApi();

  const loadColumns = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/KanbanBoardColumn/recruiter/${recruiterId}`);
      if (response && Array.isArray(response)) {
        const sorted = response.sort((a, b) => a.sequence - b.sequence);
        setColumns(sorted);
      }
    } catch (error) {
      console.error("Failed to load kanban columns:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadColumns();
  }, [recruiterId]);

  const handleAddColumn = async () => {
    if (!formData.columnName.trim()) return;

    try {
      const response = await api.post(`/KanbanBoardColumn/recruiter/${recruiterId}`, {
        columnName: formData.columnName,
        recruiterId: recruiterId,
      });

      if (response) {
        setFormData({ columnName: "" });
        setIsDialogOpen(false);
        await loadColumns();
        onColumnsUpdate?.();
      }
    } catch (error) {
      console.error("Failed to create column:", error);
    }
  };

  const handleUpdateColumn = async () => {
    if (!editingColumn || !formData.columnName.trim()) return;

    try {
      const response = await api.put(`/KanbanBoardColumn/${editingColumn.id}`, {
        columnName: formData.columnName,
        isVisible: editingColumn.isVisible,
      });

      if (response) {
        setFormData({ columnName: "" });
        setEditingColumn(null);
        setIsDialogOpen(false);
        await loadColumns();
        onColumnsUpdate?.();
      }
    } catch (error) {
      console.error("Failed to update column:", error);
    }
  };

  const handleDeleteColumn = async (columnId: string) => {
    if (!confirm("Are you sure you want to delete this column?")) return;

    try {
      await api.delete(`/KanbanBoardColumn/${columnId}`);
      await loadColumns();
      onColumnsUpdate?.();
    } catch (error) {
      console.error("Failed to delete column:", error);
    }
  };

  const handleToggleVisibility = async (column: KanbanBoardColumn) => {
    try {
      const response = await api.put(`/KanbanBoardColumn/${column.id}`, {
        columnName: column.columnName,
        isVisible: !column.isVisible,
      });

      if (response) {
        await loadColumns();
        onColumnsUpdate?.();
      }
    } catch (error) {
      console.error("Failed to toggle visibility:", error);
    }
  };

  const openEditDialog = (column: KanbanBoardColumn) => {
    setEditingColumn(column);
    setFormData({ columnName: column.columnName });
    setIsDialogOpen(true);
  };

  const openAddDialog = () => {
    setEditingColumn(null);
    setFormData({ columnName: "" });
    setIsDialogOpen(true);
  };

  const closeDialog = () => {
    setIsDialogOpen(false);
    setEditingColumn(null);
    setFormData({ columnName: "" });
  };

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <CardTitle>Kanban Board Columns</CardTitle>
        <Button onClick={openAddDialog} size="sm" variant="outline">
          <Plus className="h-4 w-4 mr-2" />
          Add Column
        </Button>
      </CardHeader>

      <CardContent>
        {loading ? (
          <div className="flex justify-center py-8">Loading columns...</div>
        ) : columns.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground">
            No columns yet. Create one to get started.
          </div>
        ) : (
          <div className="space-y-2">
            {columns.map((column) => (
              <div
                key={column.id}
                className="flex items-center gap-3 p-3 rounded-lg border hover:bg-muted/50 transition-colors"
              >
                <GripVertical className="h-4 w-4 text-muted-foreground flex-shrink-0" />
                
                <div className="flex-1">
                  <p className="font-medium">{column.columnName}</p>
                  <p className="text-xs text-muted-foreground">Sequence: {column.sequence}</p>
                </div>

                <div className="flex items-center gap-2">
                  <Button
                    onClick={() => handleToggleVisibility(column)}
                    variant="ghost"
                    size="sm"
                  >
                    {column.isVisible ? (
                      <Eye className="h-4 w-4" />
                    ) : (
                      <EyeOff className="h-4 w-4" />
                    )}
                  </Button>

                  <Button
                    onClick={() => openEditDialog(column)}
                    variant="ghost"
                    size="sm"
                  >
                    <Edit2 className="h-4 w-4" />
                  </Button>

                  <Button
                    onClick={() => handleDeleteColumn(column.id)}
                    variant="ghost"
                    size="sm"
                    className="text-destructive hover:text-destructive"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>

      {/* Dialog for Add/Edit Column */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editingColumn ? "Edit Column" : "Add New Column"}
            </DialogTitle>
            <DialogDescription>
              {editingColumn
                ? "Update the column name"
                : "Create a new kanban board column"}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium">Column Name</label>
              <Input
                placeholder="e.g., Draft, Open, In Review..."
                value={formData.columnName}
                onChange={(e) =>
                  setFormData({ ...formData, columnName: e.target.value })
                }
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={closeDialog}>
              Cancel
            </Button>
            <Button
              onClick={
                editingColumn ? handleUpdateColumn : handleAddColumn
              }
              disabled={!formData.columnName.trim()}
            >
              {editingColumn ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  );
}
