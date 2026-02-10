"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Button, buttonVariants } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Edit, ArrowLeft, Tag } from "lucide-react";
import Link from "next/link";

import { useApi } from "@/hooks/useApi";
import { Prompt } from "@/types/prompt";
import { formatDate } from "@/lib/utils";
import { toast } from "sonner";

export default function PromptDetailPage() {
  const params = useParams();
  const router = useRouter();
  const api = useApi();
  
  const [prompt, setPrompt] = useState<Prompt | null>(null);
  const [loading, setLoading] = useState(true);

  const name = params.name as string;
  const versionParam = params.version as string;
  const isLatest = versionParam === "latest";
  const versionNumber = isLatest ? undefined : Number.parseInt(versionParam, 10);

  useEffect(() => {
    const fetchPrompt = async () => {
      try {
        const response = await api.get(
          isLatest ? `/prompt/${name}/latest` : `/prompt/${name}/${versionNumber}`
        );
        // Handle both ApiResponse wrapper and direct data
        const data = response.data || response;
        if (data && data.name) {
          setPrompt(data);
        }
      } catch (error) {
        toast.error('Failed to fetch prompt: ' + error);
      } finally {
        setLoading(false);
      }
    };

    if (name && (isLatest || Number.isFinite(versionNumber))) {
      fetchPrompt();
    } else {
      setLoading(false);
      setPrompt(null);
    }
  }, [name, isLatest, versionNumber]);

  if (loading) {
    return <div>Loading...</div>;
  }

  if (!prompt) {
    return <div>Prompt not found</div>;
  }

  return (
    <div className="container py-8">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Back Button */}
        <div>
          <Link
            href="/recruiter/prompts"
            className={buttonVariants({ variant: "ghost" })}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Prompts
          </Link>
        </div>

        {/* Header */}
        <div className="space-y-4">
          <div className="flex items-start justify-between">
            <div className="space-y-2">
              <h1 className="text-3xl font-bold">{prompt.name}</h1>
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="font-mono">
                  v{prompt.version}
                </Badge>
                <Badge variant="outline" className="capitalize">
                  {prompt.category}
                </Badge>
                {prompt.locale && (
                  <Badge variant="secondary" className="text-xs">
                    {prompt.locale}
                  </Badge>
                )}
              </div>
            </div>
            
            <Link
              href={`/recruiter/prompts/${prompt.name}/${prompt.version}/edit`}
              className={buttonVariants({ variant: "outline", size: "sm" })}
            >
              <Edit className="mr-2 h-4 w-4" />
              Edit Prompt
            </Link>
          </div>
        </div>

        {/* Content */}
        <Card>
          <CardHeader>
            <CardTitle>Prompt Content</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="whitespace-pre-wrap text-sm leading-relaxed">
              {prompt.content}
            </div>
          </CardContent>
        </Card>

        {/* Tags */}
        {prompt.tags.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Tag className="h-4 w-4" />
                Tags
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-wrap gap-2">
                {prompt.tags.map((tag, index) => (
                  <Badge key={index} variant="outline">
                    {tag}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>
        )}

        {/* Metadata */}
        <Card>
          <CardHeader>
            <CardTitle>Metadata</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
              <div>
                <span className="font-medium">Created:</span>
                <p className="text-muted-foreground">{formatDate(prompt.createdAt)}</p>
              </div>
              <div>
                <span className="font-medium">Updated:</span>
                <p className="text-muted-foreground">{formatDate(prompt.updatedAt)}</p>
              </div>
              {prompt.createdBy && (
                <div>
                  <span className="font-medium">Created By:</span>
                  <p className="text-muted-foreground">{prompt.createdBy}</p>
                </div>
              )}
              {prompt.updatedBy && (
                <div>
                  <span className="font-medium">Updated By:</span>
                  <p className="text-muted-foreground">{prompt.updatedBy}</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
