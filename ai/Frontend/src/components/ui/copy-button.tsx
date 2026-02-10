import { Check, Clipboard } from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { toast } from "sonner";

interface CopyButtonProps
  extends React.ComponentPropsWithoutRef<typeof Button> {
  text: string;
  className?: string;
  successMessage?: string;
  children?: React.ReactNode;
}
export function CopyButton({
  text,
  children,
  successMessage = "Copied to clipboard!",
  ...props
}: CopyButtonProps) {
  const [copied, setCopied] = useState(false);

  const content = children || <Clipboard />;

  const copyToClipboard = async () => {
    try {
      await navigator.clipboard.writeText(text);

      toast.success(successMessage);

      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error("Failed to copy text:", err);
    }
  };

  return (
    <Button variant="outline" size="icon" {...props} onClick={copyToClipboard}>
      {copied ? <Check className="h-4 w-4" /> : content}
    </Button>
  );
}
