"use client";

import { Badge } from "@/components/ui/badge";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { InterviewConfigurationWithPrompts } from "@/types/interviewConfiguration";
import { formatDate } from "@/lib/utils";
import { Edit, ArrowLeft } from "lucide-react";
import Link from "next/link";

interface InterviewConfigurationViewProps {
  configuration: InterviewConfigurationWithPrompts;
}

export default function InterviewConfigurationView({ configuration }: InterviewConfigurationViewProps) {
  const PromptSection = ({ 
    title, 
    promptName, 
    promptVersion, 
    prompt 
  }: { 
    title: string;
    promptName: string;
    promptVersion?: number;
    prompt?: any;
  }) => (
    <Card>
      <CardHeader className="pb-3">
        <CardTitle className="text-lg">{title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        <div className="flex items-center gap-2">
          <span className="font-medium">Name:</span>
          <span className="text-sm">{promptName}</span>
          {promptVersion && (
            <Badge variant="secondary" className="font-mono">
              v{promptVersion}
            </Badge>
          )}
          {!promptVersion && (
            <Badge variant="outline">Latest Version</Badge>
          )}
        </div>
        
        {prompt && (
          <>
            <Separator />
            <div className="space-y-2">
              <div className="flex items-center gap-2">
                <span className="font-medium text-sm">Category:</span>
                <Badge variant="outline" className="capitalize">
                  {prompt.category}
                </Badge>
              </div>
              
              {prompt.locale && (
                <div className="flex items-center gap-2">
                  <span className="font-medium text-sm">Locale:</span>
                  <Badge variant="secondary" className="text-xs">
                    {prompt.locale}
                  </Badge>
                </div>
              )}
              
              {prompt.tags && prompt.tags.length > 0 && (
                <div className="space-y-1">
                  <span className="font-medium text-sm">Tags:</span>
                  <div className="flex flex-wrap gap-1">
                    {prompt.tags.map((tag: string, index: number) => (
                      <Badge key={index} variant="outline" className="text-xs">
                        {tag}
                      </Badge>
                    ))}
                  </div>
                </div>
              )}
              
              <div className="space-y-1">
                <span className="font-medium text-sm">Content:</span>
                <div className="p-3 bg-muted rounded-lg">
                  <p className="text-sm whitespace-pre-wrap">{prompt.content}</p>
                </div>
              </div>
              
              <div className="text-xs text-muted-foreground">
                Created: {formatDate(prompt.createdAt)}
              </div>
            </div>
          </>
        )}
        
        {!prompt && (
          <div className="text-sm text-muted-foreground italic">
            Prompt details not available
          </div>
        )}
      </CardContent>
    </Card>
  );

  return (
    <div className="container py-8">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Back Button */}
        <div>
          <Link
            href="/recruiter/interviewConfigurations"
            className={buttonVariants({ variant: "ghost" })}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Interview Configurations
          </Link>
        </div>

        {/* Header */}
        <div className="space-y-4">
          <div className="flex items-start justify-between">
            <div className="space-y-2">
              <h1 className="text-3xl font-bold">{configuration.name}</h1>
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="font-mono">
                  v{configuration.version}
                </Badge>
                <Badge variant={configuration.active ? "default" : "secondary"}>
                  {configuration.active ? "Active" : "Inactive"}
                </Badge>
                <Badge variant="outline" className="capitalize">
                  {configuration.modality}
                </Badge>
              </div>
            </div>
            
            <Link
              href={`/recruiter/interviewConfigurations/${configuration.name}/${configuration.version}/edit`}
              className={buttonVariants({ variant: "outline", size: "sm" })}
            >
              <Edit className="mr-2 h-4 w-4" />
              Edit Configuration
            </Link>
          </div>
        </div>

        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle>Configuration Details</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              <div className="space-y-1">
                <span className="font-medium text-sm text-muted-foreground">Modality</span>
                <p className="capitalize">{configuration.modality}</p>
              </div>
              
              {configuration.tone && (
                <div className="space-y-1">
                  <span className="font-medium text-sm text-muted-foreground">Tone</span>
                  <p>{configuration.tone}</p>
                </div>
              )}
              
              {configuration.probingDepth && (
                <div className="space-y-1">
                  <span className="font-medium text-sm text-muted-foreground">Probing Depth</span>
                  <p>{configuration.probingDepth}</p>
                </div>
              )}
              
              {configuration.focusArea && (
                <div className="space-y-1">
                  <span className="font-medium text-sm text-muted-foreground">Focus Area</span>
                  <p>{configuration.focusArea}</p>
                </div>
              )}
              
              {configuration.duration && (
                <div className="space-y-1">
                  <span className="font-medium text-sm text-muted-foreground">Duration</span>
                  <p>{configuration.duration} minutes</p>
                </div>
              )}
              
              {configuration.language && (
                <div className="space-y-1">
                  <span className="font-medium text-sm text-muted-foreground">Language</span>
                  <p>{configuration.language}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Metadata */}
        <Card>
          <CardHeader>
            <CardTitle>Metadata</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
              <div>
                <span className="font-medium">Created:</span>
                <p className="text-muted-foreground">{formatDate(configuration.createdAt)}</p>
              </div>
              <div>
                <span className="font-medium">Updated:</span>
                <p className="text-muted-foreground">{formatDate(configuration.updatedAt)}</p>
              </div>
              {configuration.createdBy && (
                <div>
                  <span className="font-medium">Created By:</span>
                  <p className="text-muted-foreground">{configuration.createdBy}</p>
                </div>
              )}
              {configuration.updatedBy && (
                <div>
                  <span className="font-medium">Updated By:</span>
                  <p className="text-muted-foreground">{configuration.updatedBy}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Prompt Configurations */}
        <div className="space-y-4">
          <h2 className="text-2xl font-semibold">Prompt Configurations</h2>
          
          <div className="grid grid-cols-1 gap-4">
            <PromptSection
              title="Instruction Prompt"
              promptName={configuration.instructionPromptName}
              promptVersion={configuration.instructionPromptVersion}
              prompt={configuration.instructionPrompt}
            />
            
            <PromptSection
              title="Personality Prompt"
              promptName={configuration.personalityPromptName}
              promptVersion={configuration.personalityPromptVersion}
              prompt={configuration.personalityPrompt}
            />
            
            <PromptSection
              title="Questions Prompt"
              promptName={configuration.questionsPromptName}
              promptVersion={configuration.questionsPromptVersion}
              prompt={configuration.questionsPrompt}
            />
          </div>
        </div>
      </div>
    </div>
  );
}

