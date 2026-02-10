"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Badge } from "@/components/ui/badge";
import { X } from "lucide-react";

import { useApi } from "@/hooks/useApi";
import { Prompt, PROMPT_CATEGORIES, COMMON_LOCALES } from "@/types/prompt";
import { promptFormSchema, PromptFormData } from "@/schemas/prompt.schema";

interface PromptFormProps {
  prompt?: Prompt;
  mode: 'create' | 'edit';
}

export default function PromptForm({ prompt, mode }: PromptFormProps) {
  const router = useRouter();
  const api = useApi();
  const [loading, setLoading] = useState(false);
  const [tagInput, setTagInput] = useState("");
  const [shouldUpdateVersion, setShouldUpdateVersion] = useState(false);

  const form = useForm<PromptFormData>({
    resolver: zodResolver(promptFormSchema),
    defaultValues: {
      name: prompt?.name || "",
      version: prompt?.version || 1,
      category: (prompt?.category as any) || "",
      content: prompt?.content || "",
      locale: prompt?.locale || "",
      tags: prompt?.tags || [],
    },
  });

  const onSubmit = async (data: PromptFormData) => {
    setLoading(true);
    try {
      if (mode === 'create') {
        await api.post('/Prompt', data);
      } else if (prompt) {
        // Include shouldUpdateVersion for edit mode
        const updateData = { ...data, shouldUpdateVersion };
        await api.put(`/Prompt/${prompt.name}/${prompt.version}`, updateData);
      }
      
      router.push('/recruiter/prompts');
    } catch (error) {
      console.error('Failed to save prompt:', error);
    } finally {
      setLoading(false);
    }
  };

  const addTag = () => {
    const currentTags = form.getValues('tags') || [];
    if (tagInput.trim() && !currentTags.includes(tagInput.trim())) {
      form.setValue('tags', [...currentTags, tagInput.trim()]);
      setTagInput("");
    }
  };

  const removeTag = (tagToRemove: string) => {
    const currentTags = form.getValues('tags') || [];
    form.setValue('tags', currentTags.filter(tag => tag !== tagToRemove));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      addTag();
    }
  };

  return (
    <div className="container py-8">
      <div className="max-w-2xl mx-auto space-y-6">
      {/* Back Button */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="sm"
          onClick={() => router.push('/recruiter/prompts')}
          className="flex items-center gap-2"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Prompts
        </Button>
      </div>

      {/* Header */}
      <div className="space-y-2">
        <h1 className="text-3xl font-bold">
          {mode === 'create' ? 'Create New Prompt' : 'Edit Prompt'}
        </h1>
        <p className="text-muted-foreground">
          {mode === 'create' 
            ? 'Create a new prompt template for your recruitment process'
            : 'Update the prompt template details'
          }
        </p>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          {/* Name - Full width */}
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Name *</FormLabel>
                <FormControl>
                  <Input 
                    placeholder="e.g., InterviewerPersonalityPrompt" 
                    {...field}
                    disabled={mode === 'edit'}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Version, Category, Locale - Three columns */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Version */}
            <FormField
              control={form.control}
              name="version"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Version *</FormLabel>
                  <FormControl>
                    <Input 
                     className="w-full"
                      type="number" 
                      min="1"
                      {...field}
                      onChange={(e) => field.onChange(parseInt(e.target.value) || 1)}
                      disabled={mode === 'edit'}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Category */}
            <FormField
              control={form.control}
              name="category"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Category *</FormLabel>
                  <Select onValueChange={field.onChange} defaultValue={field.value}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select a category" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {PROMPT_CATEGORIES.map((category) => (
                        <SelectItem key={category} value={category}>
                          {category.charAt(0).toUpperCase() + category.slice(1)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Locale */}
            <FormField
              control={form.control}
              name="locale"
              render={({ field }) => (
                <FormItem className="w-full">
                  <FormLabel>Locale</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value || undefined}>
                    <FormControl>
                      <SelectTrigger className="w-full">
                        <SelectValue placeholder="Select a locale (optional)" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {COMMON_LOCALES.map((locale) => (
                        <SelectItem key={locale} value={locale}>
                          {locale}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          {/* Content */}
          <FormField
            control={form.control}
            name="content"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Content *</FormLabel>
                <FormControl>
                  <Textarea
                    placeholder="Enter the prompt content..."
                    className="min-h-[520px] font-mono text-sm leading-5 resize-y"
                    spellCheck={false}
                    wrap="off"
                    onPaste={(e) => {
                      const pasted = e.clipboardData?.getData("text") ?? "";
                      // If user pastes an escaped string (e.g. JSON with \n), convert to real newlines for readability.
                      if (pasted.includes("\\n") && !pasted.includes("\n")) {
                        const converted = pasted.replaceAll("\\r\\n", "\n").replaceAll("\\n", "\n").replaceAll("\\t", "\t");
                        e.preventDefault();

                        const el = e.currentTarget;
                        const start = el.selectionStart ?? field.value.length;
                        const end = el.selectionEnd ?? field.value.length;
                        const next =
                          (field.value ?? "").slice(0, start) +
                          converted +
                          (field.value ?? "").slice(end);

                        form.setValue("content", next, { shouldDirty: true, shouldTouch: true, shouldValidate: true });
                        requestAnimationFrame(() => {
                          const pos = start + converted.length;
                          el.selectionStart = pos;
                          el.selectionEnd = pos;
                        });
                      }
                    }}
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Tags */}
          <div className="space-y-2">
            <Label>Tags</Label>
            <div className="flex gap-2">
              <Input
                placeholder="Add a tag..."
                value={tagInput}
                onChange={(e) => setTagInput(e.target.value)}
                onKeyPress={handleKeyPress}
              />
              <Button type="button" onClick={addTag} variant="outline">
                Add
              </Button>
            </div>
            
            <div className="flex flex-wrap gap-2">
              {(form.watch('tags') || []).map((tag, index) => (
                <Badge key={index} variant="secondary" className="flex items-center gap-1">
                  {tag}
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="h-auto p-0 ml-1"
                    onClick={() => removeTag(tag)}
                  >
                    <X className="h-3 w-3" />
                  </Button>
                </Badge>
              ))}
            </div>
          </div>

          {/* Update Version Checkbox - Only in edit mode */}
          {mode === 'edit' && (
            <div className="flex flex-row items-start space-x-3 space-y-0 gap-3">
              <Checkbox
                id="shouldUpdateVersion"
                checked={shouldUpdateVersion}
                onCheckedChange={(checked) => setShouldUpdateVersion(checked === true)}
              />
              <div className="space-y-1 leading-none">
                <Label htmlFor="shouldUpdateVersion">
                  Update version
                </Label>
                <p className="text-sm text-muted-foreground">
                  Check this to create a new version of the prompt
                </p>
              </div>
            </div>
          )}

          {/* Actions */}
          <div className="flex gap-4">
            <Button type="submit" disabled={loading}>
              {loading ? 'Saving...' : mode === 'create' ? 'Create Prompt' : 'Update Prompt'}
            </Button>
            <Button 
              type="button" 
              variant="outline" 
              onClick={() => router.push('/recruiter/prompts')}
            >
              Cancel
            </Button>
          </div>
        </form>
      </Form>
      </div>
    </div>
  );
}
