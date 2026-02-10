"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Textarea } from "@/components/ui/textarea";

import { CandidateCommentForm } from "@/schemas/recruiter-comment";
import { useApi } from "@/hooks/useApi";
import type { Comment as CommentView } from "@/types/v2/type.view";

interface CommentFormProps {
  candidateId: string;
  initialComment?: string;
  onSaved?: (comment: CommentView) => void;
}

export function CommentForm({ candidateId, initialComment, onSaved }: CommentFormProps) {
  const api = useApi();
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const form = useForm<CandidateCommentForm>({
    resolver: zodResolver(CandidateCommentForm),
    defaultValues: {
      comment: initialComment ?? "",
    },
  });

  // Keep form value in sync when initialComment arrives/changes
  // (defaultValues only used on first render)
  useEffect(() => {
    if (typeof initialComment === "string") {
      form.reset({ comment: initialComment });
    }
  }, [initialComment]);

  async function onSubmit(data: CandidateCommentForm) {
    try {
      setIsSubmitting(true);

      // Create comment payload - backend will set CreatedBy automatically
      const commentPayload = {
        content: data.comment,
        entityId: candidateId,
        entityType: 1, // CommentableEntityType.Candidate = 1
        parentCommentId: null,
      };

      // Submit comment
      const savedResponse = await api.post("/comment", commentPayload);
      const saved: CommentView = (savedResponse as any).data || (savedResponse as any);
      
      toast.success("Success", {
        description: "Comment saved successfully.",
      });

      // Reset form with saved content and notify parent
      form.reset({ comment: saved.content });
      onSaved?.(saved);
    } catch (error: any) {
      console.error("Error saving comment:", error);
      const errorMessage = error?.message || error?.errors?.[0] || "Failed to save comment. Please try again.";
      toast.error("Error", {
        description: errorMessage,
      });
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <Card>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-3">
            <FormField
              control={form.control}
              name="comment"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Comment</FormLabel>

                  <FormControl>
                    <Textarea
                      placeholder="Let us know what do you think about the Candidate..."
                      className="min-h-24 resize-none"
                      {...field}
                    />
                  </FormControl>

                  <FormMessage />
                </FormItem>
              )}
            />

            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Submitting..." : "Submit"}
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
