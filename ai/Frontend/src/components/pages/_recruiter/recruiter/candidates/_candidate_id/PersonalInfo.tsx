import { Globe, Mail, Phone } from "lucide-react";
import Link from "next/link";

import { badgeVariants } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";

import { cn } from "@/lib/utils";
import { CandidateView } from "@/types/v2/type.view";

export default function PersonalInfo({
  candidate,
}: {
  candidate: CandidateView;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Globe className="text-brand h-5 w-5" />
          Personal Information
        </CardTitle>
      </CardHeader>

      <CardContent className="space-y-3">
        <div>
          <label className="text-muted-foreground text-sm font-medium">
            Age
          </label>

          <p className="text-sm">{candidate.userProfile.age} years old</p>
        </div>

        <div>
          <label className="text-muted-foreground text-sm font-medium">
            Nationality
          </label>

          <p className="text-sm">{candidate.userProfile.nationality}</p>
        </div>

        <Separator />

        <div className="flex items-center gap-4 *:grow">
          <Link
            href={`mailto:${candidate.userProfile.email}`}
            target="_blank"
            className={cn(
              badgeVariants(),
              "flex items-center justify-start gap-2 [&>svg]:size-4",
            )}
          >
            <Mail />
            <span className="text-sm">{candidate.userProfile.email}</span>
          </Link>

          <Link
            href={`tel:${candidate.userProfile.phoneNumber}`}
            target="_blank"
            className={cn(
              badgeVariants(),
              "flex items-center justify-start gap-2 [&>svg]:size-4",
            )}
          >
            <Phone />
            <span className="text-sm">{candidate.userProfile.phoneNumber}</span>
          </Link>
        </div>
      </CardContent>
    </Card>
  );
}
