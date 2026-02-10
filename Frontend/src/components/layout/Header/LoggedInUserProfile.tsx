"use client";

import Link from "next/link";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button, buttonVariants } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { useAuthStore } from "@/stores/useAuthStore";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { useRouter } from "next/navigation";
import { usePathname } from "next/navigation";

const LINKS = [
  { label: "Profile", href: "/profile" },
  { label: "Notifications", href: "/notifications" },
];

export default function LoggedInUserProfile() {
  const { user: currentUser } = useAuthStore();
  const { logout } = useUnifiedAuth();
  const router = useRouter();
  const pathname = usePathname();

  const redirectQuery =
    pathname && pathname.startsWith("/interview/")
      ? `?redirect=${encodeURIComponent(pathname)}`
      : "";

  const handleLogout = async () => {
    await logout();
    // Note: logout() already handles navigation to home
  };

  return (
    <div className="flex items-center gap-x-2">
      {currentUser ? (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" className="rounded-full pl-0.5">
              <Avatar className="border">
                <AvatarImage src={currentUser.profilePictureUrl ?? undefined} />
                <AvatarFallback>
                  {currentUser.name?.charAt(0).toUpperCase() || "U"}
                </AvatarFallback>
              </Avatar>

              <span className="xs:block hidden text-sm font-medium">
                {currentUser.name ?? ""}
              </span>
            </Button>
          </DropdownMenuTrigger>

          <DropdownMenuContent>
            {LINKS.map((link) => (
              <DropdownMenuItem
                key={link.label}
                asChild
                className="cursor-pointer"
              >
                <Link href={link.href}>{link.label}</Link>
              </DropdownMenuItem>
            ))}

            <DropdownMenuSeparator />

            <DropdownMenuItem
              variant="destructive"
              className="cursor-pointer"
              onClick={handleLogout}
            >
              Logout
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      ) : (
        <div className="flex items-center gap-2">
          <Link
            href={`/sign-in${redirectQuery}`}
            className={buttonVariants({ variant: "outline" })}
          >
            Login
          </Link>

          <Link href={`/sign-up${redirectQuery}`} className={buttonVariants()}>
            Sign Up
          </Link>
        </div>
      )}
    </div>
  );
}
