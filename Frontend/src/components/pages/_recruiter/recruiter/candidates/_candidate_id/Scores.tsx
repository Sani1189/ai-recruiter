"use client";

import { useState } from "react";
import { AlignHorizontalJustifyStart } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

import { cn } from "@/lib/utils";
import { CandidateView } from "@/types/v2/type.view";
import { getGrade, getScoreColor } from "./lib/utils";

type Scoring = { fixedCategory?: string; category?: string; score?: number | null };

export default function Scores({
  candidate,
  scorings,
  rawScorings = [],
}: {
  candidate: CandidateView;
  scorings: Scoring[];
  rawScorings?: Scoring[];
}) {
  const [showDetails, setShowDetails] = useState(false);

  const safeRaw = rawScorings && rawScorings.length ? rawScorings : scorings;

  // Group raw scorings by fixedCategory (case-insensitive)
  const groupedByFixedCategory = new Map<string, Scoring[]>();
  safeRaw.forEach((s) => {
    const fixedCat = (s.fixedCategory || s.category || "Other").toString().trim();
    const normalized = fixedCat.toLowerCase();
    if (!groupedByFixedCategory.has(normalized)) {
      groupedByFixedCategory.set(normalized, []);
    }
    groupedByFixedCategory.get(normalized)!.push(s);
  });

  const rows =
    scorings && scorings.length
      ? [
          { label: "Average", value: candidate.score.avg, fixedCategory: "Average" },
          ...scorings.map((s) => ({
            label: s.fixedCategory || s.category || "N/A",
            value: s.score ?? 0,
            fixedCategory: (s.fixedCategory || s.category || "N/A").toString().trim(),
          })),
        ]
      : [
          { label: "Average", value: candidate.score.avg, fixedCategory: "Average" },
          { label: "Technical", value: candidate.score.technical, fixedCategory: "Technical" },
          { label: "Language Skills", value: candidate.score.english, fixedCategory: "Language Skills" },
          { label: "Communication", value: candidate.score.communication, fixedCategory: "Communication" },
          { label: "Problem Solving", value: candidate.score.problemSolving, fixedCategory: "Problem Solving" },
        ];

  const getCategoryDetails = (fixedCategory: string): Scoring[] => {
    const normalized = fixedCategory.trim().toLowerCase();
    return groupedByFixedCategory.get(normalized) || [];
  };

  return (
    <TooltipProvider>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between gap-2">
            <CardTitle className="flex items-center gap-2">
              <AlignHorizontalJustifyStart className="text-brand h-5 w-5" />
              Scores
            </CardTitle>
            <div className="flex items-center gap-2">
              <Label htmlFor="show-details" className="text-sm text-muted-foreground cursor-pointer">
                Show Details
              </Label>
              <Switch
                id="show-details"
                checked={showDetails}
                onCheckedChange={setShowDetails}
              />
            </div>
          </div>
        </CardHeader>

        {/* <CardContent className="grid grid-cols-2 gap-3 items-start"> */}
        <CardContent className="grid grid-cols-2 gap-3">
          {rows.map((score, index) => {
            const categoryDetails = getCategoryDetails(score.fixedCategory);

            return (
              <div
                key={index}
                className={cn(
                  // "rounded-lg border text-sm flex flex-col",
                  "rounded-lg border text-sm",
                  {
                    "border-green-500 bg-green-500/5":
                      getGrade(score.value) === "A",
                    "border-blue-500 bg-blue-500/5": getGrade(score.value) === "B",
                    "border-yellow-500 bg-yellow-500/5":
                      getGrade(score.value) === "C",
                    "border-red-500 bg-red-500/5": getGrade(score.value) === "D",
                  },
                  {
                    "col-span-2": index === 0, // Make the first score span both columns
                  },
                )}
              >
                <div
                  className={cn(
                    "flex w-full items-center justify-between gap-4 px-3 py-1.5",
                  )}
                >
                  <div className="flex items-center gap-2 flex-1 min-w-0">
                    {score.label.length > 15 ? (
                      <Tooltip>
                        <TooltipTrigger asChild>
                          <span className="font-medium truncate cursor-help">{score.label}</span>
                        </TooltipTrigger>
                        <TooltipContent>
                          <p>{score.label}</p>
                        </TooltipContent>
                      </Tooltip>
                    ) : (
                      <span className="font-medium truncate">{score.label}</span>
                    )}
                  </div>

                  <div className="flex items-center gap-2 shrink-0">
                    <div
                      className={`h-2 w-2 rounded-full ${getScoreColor(score.value)}`}
                    ></div>
                    <span className="font-semibold">{score.value}/10</span>
                  </div>
                </div>

                {showDetails && categoryDetails.length > 0 && (
                  <div className="border-t bg-muted/20 px-3 py-2 space-y-1.5">
                    {categoryDetails.map((detail, detailIndex) => (
                      <div
                        key={detailIndex}
                        className="flex items-center justify-between text-xs"
                      >
                        <span className="text-muted-foreground">
                          {detail.category || "N/A"}
                        </span>
                        <div className="flex items-center gap-1.5">
                          <div
                            className={`h-1.5 w-1.5 rounded-full ${getScoreColor(detail.score ?? 0)}`}
                          ></div>
                          <span className="font-medium">{detail.score ?? 0}/10</span>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            );
          })}
        </CardContent>
      </Card>
    </TooltipProvider>
  );
}
