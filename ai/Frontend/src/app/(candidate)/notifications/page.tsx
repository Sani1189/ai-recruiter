"use client";

import { formatDistanceToNow } from "date-fns";
import {
  AlertTriangle,
  Bell,
  Briefcase,
  CheckCircle,
  Circle,
  ExternalLink,
  Megaphone,
  MessageSquare,
} from "lucide-react";
import { useEffect } from "react";
import { useState } from "react";
import { toast } from "sonner";
import { useRouter } from "next/navigation";

import { useAuthStore } from "@/stores/useAuthStore";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { UnifiedGuard } from "@/components/auth/UnifiedGuard";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

import { mockNotifications } from "@/dummy";
import { CandidateNotificationType } from "@/enums";
import { cn } from "@/lib/utils";
import { UserNotification } from "@/types/type";

const getNotificationIcon = (type: keyof typeof CandidateNotificationType) => {
  switch (type) {
    case CandidateNotificationType.FEEDBACK_AVAILABLE:
      return MessageSquare;
    case CandidateNotificationType.NEW_INTERVIEW_PUBLISHED:
      return Briefcase;
    case CandidateNotificationType.PROFILE_INCOMPLETE:
      return AlertTriangle;
    case CandidateNotificationType.SYSTEM_ANNOUNCEMENT:
      return Megaphone;
    case CandidateNotificationType.ACCOUNT_STATUS_UPDATE:
      return CheckCircle;
    default:
      return Bell;
  }
};

const getNotificationColor = (type: keyof typeof CandidateNotificationType) => {
  switch (type) {
    case CandidateNotificationType.FEEDBACK_AVAILABLE:
      return "text-blue-500";
    case CandidateNotificationType.NEW_INTERVIEW_PUBLISHED:
      return "text-green-500";
    case CandidateNotificationType.PROFILE_INCOMPLETE:
      return "text-orange-500";
    case CandidateNotificationType.SYSTEM_ANNOUNCEMENT:
      return "text-purple-500";
    case CandidateNotificationType.ACCOUNT_STATUS_UPDATE:
      return "text-emerald-500";
    default:
      return "text-gray-500";
  }
};

export default function NotificationsPage() {
  const { user } = useAuthStore();
  const { isLoading } = useUnifiedAuth();
  const router = useRouter();
  const [notifications, setNotifications] = useState(mockNotifications);
  const [filter, setFilter] = useState<"all" | "unread">("all");

  // Check authentication
  useEffect(() => {
    if (isLoading) return; // Wait for auth to load
    
    if (!user) {
      // User not authenticated, redirect to sign in
      toast.error('Authentication required', {
        description: 'Please sign in to view your notifications.',
      });
      router.push('/sign-in');
      return;
    }
  }, [user, isLoading, router]);

  const markAllAsRead = async () => {
    // Here we would typically make an API call to mark all notifications as read
    setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
    setFilter("all");

    toast.success("All notifications marked as read", {
      description: "You can find them in the 'All' tab.",
    });
  };

  const markAsRead = async (notificationId: string) => {
    // Here we would typically make an API call to mark the notification as read
    setNotifications((prev) =>
      prev.map((n) => (n.id === notificationId ? { ...n, read: true } : n)),
    );

    toast.success("Notification marked as read", {
      description: "You can find it in the 'All' tab.",
    });
  };

  const filteredNotifications =
    filter === "unread" ? notifications.filter((n) => !n.read) : notifications;

  const unreadCount = notifications.filter((n) => !n.read).length;

  // Show loading while checking auth
  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Loading notifications...</p>
        </div>
      </div>
    );
  }

  // Show loading if no authenticated user
  if (!user) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-muted-foreground">Redirecting to sign in...</p>
        </div>
      </div>
    );
  }

  return (
      <div className="from-primary/5 via-background to-secondary/5 min-h-screen bg-gradient-to-br">
        <div className="container space-y-8 px-24 py-8">
          <div className="flex items-center justify-between">
            <div className="space-y-2">
              <h1 className="text-3xl font-bold">Notifications</h1>
              <p className="text-muted-foreground">
                Stay updated with your interview progress and platform updates
              </p>
            </div>

            {unreadCount > 0 && (
              <Button onClick={markAllAsRead}>Mark all as read</Button>
            )}
          </div>

          <Tabs
            value={filter}
            onValueChange={(value) => setFilter(value as "all" | "unread")}
            className="space-y-6"
          >
            <TabsList>
              <TabsTrigger value="all" className="flex items-center gap-2">
                <Bell className="h-4 w-4" />
                All ({notifications.length})
              </TabsTrigger>

              <TabsTrigger value="unread" className="flex items-center gap-2">
                <Circle className="h-4 w-4" />
                Unread ({unreadCount})
              </TabsTrigger>
            </TabsList>

            <TabsContent value={filter}>
              {filteredNotifications.length === 0 ? (
                <Card>
                  <CardContent className="py-12 pt-6 text-center">
                    <Bell className="text-muted-foreground mx-auto mb-4 h-12 w-12" />
                    <h3 className="mb-2 text-lg font-semibold">
                      {filter === "unread"
                        ? "No unread notifications"
                        : "No notifications"}
                    </h3>
                    <p className="text-muted-foreground">
                      {filter === "unread"
                        ? "You're all caught up! No unread notifications."
                        : "You don't have any notifications yet."}
                    </p>
                  </CardContent>
                </Card>
              ) : (
                <div className="space-y-4">
                  {filteredNotifications.map((notification) => (
                    <NotificationCard
                      key={notification.id}
                      notification={notification}
                      markAsRead={markAsRead}
                    />
                  ))}
                </div>
              )}
            </TabsContent>
          </Tabs>
        </div>
      </div>
  );
}

const NotificationCard = ({
  notification,
  markAsRead,
}: {
  notification: UserNotification;
  markAsRead: (id: string) => void;
}) => {
  const Icon = getNotificationIcon(notification.type);
  const iconColor = getNotificationColor(notification.type);

  return (
    <Card
      key={notification.id}
      className={cn("transition-colors", {
        "bg-primary/5 border-primary/20": !notification.read,
      })}
    >
      <CardContent className="flex items-start gap-4">
        <div className={`flex-shrink-0 ${iconColor}`}>
          <Icon className="h-5 w-5" />
        </div>

        <div className="flex grow items-start justify-between gap-4">
          <div>
            <h3 className="mb-1 text-sm font-semibold">
              {notification.title}
              {!notification.read && (
                <Badge variant="secondary" className="ml-2">
                  New
                </Badge>
              )}
            </h3>

            <p className="text-muted-foreground mb-2 text-sm">
              {notification.message}
            </p>

            <p className="text-muted-foreground text-xs">
              {formatDistanceToNow(notification.timestamp, {
                addSuffix: true,
              })}
            </p>
          </div>

          <div className="flex gap-2">
            {notification.actionUrl && (
              <Button variant="outline" size="sm">
                <ExternalLink className="mr-1 h-3 w-3" />
                View
              </Button>
            )}

            {!notification.read && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => markAsRead(notification.id)}
              >
                Mark as read
              </Button>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
