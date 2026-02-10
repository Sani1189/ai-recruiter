"use client";

import { useState, useEffect } from "react";
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

import { useApi } from "@/hooks/useApi";
import { 
  InterviewConfiguration, 
  INTERVIEW_MODALITIES, 
  COMMON_LANGUAGES,
  PromptVersion 
} from "@/types/interviewConfiguration";
import { 
  interviewConfigurationFormSchema, 
  InterviewConfigurationFormData 
} from "@/schemas/interviewConfiguration.schema";

interface InterviewConfigurationFormProps {
  configuration?: InterviewConfiguration;
  mode: 'create' | 'edit';
}

export default function InterviewConfigurationForm({ configuration, mode }: InterviewConfigurationFormProps) {
  const router = useRouter();
  const api = useApi();
  const [loading, setLoading] = useState(false);
  const [shouldUpdateVersion, setShouldUpdateVersion] = useState(false);
  
  // State for available prompts
  const [availablePrompts, setAvailablePrompts] = useState<string[]>([]);
  
  // State for prompt versions
  const [availableVersions, setAvailableVersions] = useState<{
    instruction: PromptVersion[];
    personality: PromptVersion[];
    questions: PromptVersion[];
  }>({
    instruction: [],
    personality: [],
    questions: []
  });

  // State for "use latest" checkboxes
  const [useLatest, setUseLatest] = useState({
    instruction: true,
    personality: true,
    questions: true
  });

  const form = useForm<InterviewConfigurationFormData>({
    resolver: zodResolver(interviewConfigurationFormSchema) as any,
    defaultValues: {
      name: configuration?.name || "",
      version: configuration?.version || 1,
      modality: (configuration?.modality as any) || "",
      tone: configuration?.tone || "",
      probingDepth: configuration?.probingDepth || "",
      focusArea: configuration?.focusArea || "",
      duration: configuration?.duration || undefined,
      language: (configuration?.language as any) || "",
      instructionPromptName: configuration?.instructionPromptName || "",
      instructionPromptVersion: configuration?.instructionPromptVersion || undefined,
      personalityPromptName: configuration?.personalityPromptName || "",
      personalityPromptVersion: configuration?.personalityPromptVersion || undefined,
      questionsPromptName: configuration?.questionsPromptName || "",
      questionsPromptVersion: configuration?.questionsPromptVersion || undefined,
      active: configuration?.active ?? true,
    },
  });

  // Load available prompts
  const loadAvailablePrompts = async () => {
    try {
      const response = await api.get('/Prompt');
      // The response is already an array, not nested under .data
      const prompts = Array.isArray(response) ? response : (response.data || []);
      const uniqueNames = [...new Set(prompts.map((p: any) => p.name as string))] as string[];
      setAvailablePrompts(uniqueNames);
    } catch (error) {
      console.error('Failed to load available prompts:', error);
      setAvailablePrompts([]);
    }
  };

  // Load prompt versions when prompt name changes
  const loadPromptVersions = async (promptName: string, promptType: 'instruction' | 'personality' | 'questions') => {
    if (!promptName) {
      setAvailableVersions(prev => ({
        ...prev,
        [promptType]: []
      }));
      return;
    }

    try {
      const response = await api.get(`/InterviewConfiguration/prompt-versions/${promptName}`);
      // Handle response structure similar to prompts
      const versions = Array.isArray(response) ? response : (response.data || []);
      setAvailableVersions(prev => ({
        ...prev,
        [promptType]: versions
      }));
    } catch (error) {
      console.error('Failed to load prompt versions:', error);
      setAvailableVersions(prev => ({
        ...prev,
        [promptType]: []
      }));
    }
  };

  // Handle prompt name change
  const handlePromptNameChange = (promptType: 'instruction' | 'personality' | 'questions', promptName: string) => {
    form.setValue(`${promptType}PromptName` as any, promptName);
    form.setValue(`${promptType}PromptVersion` as any, undefined);
    loadPromptVersions(promptName, promptType);
  };

  // Handle use latest change
  const handleUseLatestChange = (promptType: 'instruction' | 'personality' | 'questions', useLatestValue: boolean) => {
    setUseLatest(prev => ({
      ...prev,
      [promptType]: useLatestValue
    }));
    
    if (useLatestValue) {
      form.setValue(`${promptType}PromptVersion` as any, undefined);
    }
  };


  // Load initial data
  useEffect(() => {
    loadAvailablePrompts();
    
    if (mode === 'edit' && configuration) {
      if (configuration.instructionPromptName) {
        loadPromptVersions(configuration.instructionPromptName, 'instruction');
      }
      if (configuration.personalityPromptName) {
        loadPromptVersions(configuration.personalityPromptName, 'personality');
      }
      if (configuration.questionsPromptName) {
        loadPromptVersions(configuration.questionsPromptName, 'questions');
      }
      
      // Set use latest based on whether version is provided
      setUseLatest({
        instruction: !configuration.instructionPromptVersion,
        personality: !configuration.personalityPromptVersion,
        questions: !configuration.questionsPromptVersion
      });
    }
  }, [mode, configuration]);

  const onSubmit = async (data: InterviewConfigurationFormData) => {
    setLoading(true);
    try {
      if (mode === 'create') {
        await api.post('/InterviewConfiguration', data);
      } else if (configuration) {
        // Include shouldUpdateVersion for edit mode
        const updateData = { ...data, shouldUpdateVersion };
        await api.put(`/InterviewConfiguration/${configuration.name}/${configuration.version}`, updateData);
      }
      
      router.push('/recruiter/interviewConfigurations');
    } catch (error) {
      console.error('Failed to save interview configuration:', error);
    } finally {
      setLoading(false);
    }
  };

  // Prompt Selection Component (inline)
  const PromptSelection = ({ 
    promptType, 
    label 
  }: { 
    promptType: 'instruction' | 'personality' | 'questions';
    label: string;
  }) => {
    const promptName = form.watch(`${promptType}PromptName` as any);
    const promptVersion = form.watch(`${promptType}PromptVersion` as any);
    const useLatestValue = useLatest[promptType];
    const versions = availableVersions[promptType];

    return (
      <div className="space-y-3">
        <h4 className="text-sm font-medium text-muted-foreground">{label}</h4>
        
        {/* Prompt Name - Select Dropdown */}
        <FormField
          control={form.control as any}
          name={`${promptType}PromptName` as any}
          render={({ field }) => (
            <FormItem>
              <FormLabel className="text-xs">Prompt Name *</FormLabel>
              <Select onValueChange={(value) => {
                field.onChange(value);
                handlePromptNameChange(promptType, value);
              }} value={field.value}>
                <FormControl>
                  <SelectTrigger className="h-8">
                    <SelectValue placeholder="Select prompt..." />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {availablePrompts.map(promptName => (
                    <SelectItem key={promptName} value={promptName}>
                      {promptName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Use Latest Checkbox */}
        <div className="flex items-center space-x-2">
          <Checkbox
            id={`useLatest${promptType}`}
            checked={useLatestValue}
            onCheckedChange={(checked) => handleUseLatestChange(promptType, checked === true)}
            className="h-4 w-4"
          />
          <Label htmlFor={`useLatest${promptType}`} className="text-xs">
            Use Latest
          </Label>
        </div>

        {/* Version Selection (conditional) */}
        {!useLatestValue && versions.length > 0 && (
          <FormField
            control={form.control}
            name={`${promptType}PromptVersion` as any}
            render={({ field }) => (
              <FormItem>
                <FormLabel className="text-xs">Version *</FormLabel>
                <Select onValueChange={(value) => field.onChange(Number(value))} value={field.value?.toString()}>
                  <FormControl>
                    <SelectTrigger className="h-8">
                      <SelectValue placeholder="Select version" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {versions.map(version => (
                      <SelectItem key={version.version} value={version.version.toString()}>
                        v{version.version} - {new Date(version.createdAt).toLocaleDateString()} - {version.createdBy}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        )}
      </div>
    );
  };

  return (
    <div className="container py-8">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Back Button */}
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.push('/recruiter/interviewConfigurations')}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Interview Configurations
          </Button>
        </div>

        {/* Header */}
        <div className="space-y-2">
          <h1 className="text-3xl font-bold">
            {mode === 'create' ? 'Create New Interview Configuration' : 'Edit Interview Configuration'}
          </h1>
          <p className="text-muted-foreground">
            {mode === 'create' 
              ? 'Create a new interview configuration for your recruitment process'
              : 'Update the interview configuration details'
            }
          </p>
        </div>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
            {/* Basic Information */}
            <div className="space-y-6">
              <h3 className="text-xl font-semibold">Basic Information</h3>
              
              {/* Name and Version */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Name *</FormLabel>
                      <FormControl>
                        <Input 
                          placeholder="e.g., TechnicalInterviewConfig" 
                          {...field}
                          disabled={mode === 'edit'}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="version"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Version *</FormLabel>
                      <FormControl>
                        <Input 
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
              </div>

              {/* Modality, Tone, Probing Depth */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <FormField
                  control={form.control}
                  name="modality"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Modality *</FormLabel>
                      <Select onValueChange={field.onChange} defaultValue={field.value}>
                        <FormControl>
                          <SelectTrigger className="w-full">
                            <SelectValue placeholder="Select modality" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {INTERVIEW_MODALITIES.map((modality) => (
                            <SelectItem key={modality} value={modality}>
                              {modality.charAt(0).toUpperCase() + modality.slice(1)}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="tone"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Tone</FormLabel>
                      <FormControl>
                        <Input 
                          placeholder="e.g., Professional, Friendly" 
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="probingDepth"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Probing Depth</FormLabel>
                      <FormControl>
                        <Input 
                          placeholder="e.g., Deep, Surface" 
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Focus Area, Duration, Language */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <FormField
                  control={form.control}
                  name="focusArea"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Focus Area</FormLabel>
                      <FormControl>
                        <Input 
                          placeholder="e.g., Technical Skills" 
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="duration"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Duration (minutes)</FormLabel>
                      <FormControl>
                        <Input 
                          type="number" 
                          min="1"
                          placeholder="e.g., 60" 
                          {...field}
                          onChange={(e) => field.onChange(parseInt(e.target.value) || undefined)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="language"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Language</FormLabel>
                      <Select onValueChange={field.onChange} value={field.value || undefined}>
                        <FormControl>
                          <SelectTrigger className="w-full">
                            <SelectValue placeholder="Select language (optional)" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {COMMON_LANGUAGES.map((language) => (
                            <SelectItem key={language} value={language}>
                              {language}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              {/* Active Status */}
              <FormField
                control={form.control}
                name="active"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-start space-x-3 space-y-0">
                    <FormControl>
                      <Checkbox
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                    <div className="space-y-1 leading-none">
                      <FormLabel>Active</FormLabel>
                      <p className="text-sm text-muted-foreground">
                        Enable this configuration for use
                      </p>
                    </div>
                  </FormItem>
                )}
              />
            </div>

            {/* Prompt Configurations */}
            <div className="space-y-6">
              <h3 className="text-xl font-semibold">Prompt Configurations</h3>
              
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <PromptSelection promptType="instruction" label="Instruction Prompt" />
                <PromptSelection promptType="personality" label="Personality Prompt" />
                <PromptSelection promptType="questions" label="Questions Prompt" />
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
                    Check this to create a new version of the interview configuration
                  </p>
                </div>
              </div>
            )}

            {/* Actions */}
            <div className="flex gap-4">
              <Button type="submit" disabled={loading}>
                {loading ? 'Saving...' : mode === 'create' ? 'Create Configuration' : 'Update Configuration'}
              </Button>
              <Button 
                type="button" 
                variant="outline" 
                onClick={() => router.push('/recruiter/interviewConfigurations')}
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
