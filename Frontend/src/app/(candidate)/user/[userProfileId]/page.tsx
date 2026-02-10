import {
  Briefcase,
  Building,
  Calendar,
  CheckCircle,
  Code2,
  ExternalLink,
  Globe,
  GraduationCap,
  Languages,
  Mail,
  MapPin,
  Phone,
  User,
  XCircle,
} from "lucide-react";

import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

import { mockUserProfiles } from "@/dummy";

interface UserProfilePageProps {
  params: Promise<{ userProfileId: string }>;
}
export default async function UserProfilePage({
  params,
}: UserProfilePageProps) {
  const { userProfileId } = await params;

  const getProficiencyColor = (proficiency: string) => {
    switch (proficiency) {
      case "advanced":
        return "bg-green-100 text-green-800 border-green-200";
      case "intermediate":
        return "bg-blue-100 text-blue-800 border-blue-200";
      case "beginner":
        return "bg-yellow-100 text-yellow-800 border-yellow-200";
      default:
        return "bg-gray-100 text-gray-800 border-gray-200";
    }
  };

  const userProfile = mockUserProfiles.find((p) => p.id === userProfileId);
  if (!userProfile) {
    return (
      <div className="from-primary/5 via-background to-secondary/5 flex min-h-screen items-center justify-center bg-gradient-to-br">
        <Card className="w-full max-w-md shadow-lg">
          <CardContent>
            <div className="text-center">
              <h2 className="text-foreground text-2xl font-bold">
                Profile Not Found
              </h2>

              <p className="text-muted-foreground mt-2">
                The requested profile does not exist.
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="from-primary/5 via-background to-secondary/5 min-h-screen bg-gradient-to-br">
      <div className="from-primary to-secondary text-primary-foreground relative bg-gradient-to-r">
        <div className="absolute inset-0 bg-black/20"></div>
        <div className="relative container mx-auto px-4 py-16">
          <div className="flex flex-col items-center gap-8 lg:flex-row">
            <Avatar className="ring-primary-foreground/20 h-40 w-40 shadow-2xl ring-4">
              <AvatarImage
                src={userProfile.profilePictureUrl}
                alt={userProfile.name}
              />
              <AvatarFallback className="bg-primary-foreground/10 text-4xl">
                {userProfile.name
                  .split(" ")
                  .map((n) => n[0])
                  .join("")}
              </AvatarFallback>
            </Avatar>

            <div className="flex-1 text-center lg:text-left">
              <h1 className="mb-4 text-5xl font-bold">{userProfile.name}</h1>
              <p className="text-primary-foreground/90 mb-6 max-w-2xl text-xl">
                {userProfile.bio}
              </p>

              <div className="text-primary-foreground/80 flex flex-wrap justify-center gap-4 lg:justify-start">
                <div className="flex items-center gap-2">
                  <Mail className="h-5 w-5" />
                  <span>{userProfile.email}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Phone className="h-5 w-5" />
                  <span>{userProfile.phoneNumber}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Globe className="h-5 w-5" />
                  <span>
                    {userProfile.nationality} • {userProfile.age} years old
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="container mx-auto max-w-6xl px-4 py-8">
        {/* Quick Info Cards */}
        <div className="relative z-10 -mt-16 mb-8 grid grid-cols-1 gap-6 md:grid-cols-3">
          <Card className="bg-card/95 shadow-lg backdrop-blur">
            <CardContent className="p-6 text-center">
              <Briefcase className="text-primary mx-auto mb-3 h-8 w-8" />
              <h3 className="text-foreground mb-2 font-semibold">
                Job Preferences
              </h3>
              <div className="space-y-2">
                {userProfile.jobTypePreferences.map((pref, index) => (
                  <Badge key={index} variant="secondary" className="capitalize">
                    {pref}
                  </Badge>
                ))}
              </div>
            </CardContent>
          </Card>

          <Card className="bg-card/95 shadow-lg backdrop-blur">
            <CardContent className="p-6 text-center">
              <MapPin className="text-primary mx-auto mb-3 h-8 w-8" />
              <h3 className="text-foreground mb-2 font-semibold">Work Setup</h3>
              <div className="space-y-2">
                {userProfile.remotePreferences.map((pref, index) => (
                  <Badge key={index} variant="outline" className="capitalize">
                    {pref}
                  </Badge>
                ))}
                <div className="mt-3 flex items-center justify-center gap-2">
                  {userProfile.openToRelocation ? (
                    <>
                      <CheckCircle className="h-4 w-4 text-green-600" />
                      <span className="text-sm">Open to relocation</span>
                    </>
                  ) : (
                    <>
                      <XCircle className="h-4 w-4 text-red-600" />
                      <span className="text-sm">Not relocating</span>
                    </>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>

          <Card className="bg-card/95 shadow-lg backdrop-blur">
            <CardContent className="p-6 text-center">
              <User className="text-primary mx-auto mb-3 h-8 w-8" />
              <h3 className="text-foreground mb-2 font-semibold">Experience</h3>
              <div className="text-primary mb-1 text-2xl font-bold">
                {userProfile.workExperience.length}
              </div>
              <div className="text-muted-foreground text-sm">
                Companies worked with
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Main Content Tabs */}
        <Tabs defaultValue="experience" className="space-y-6">
          <TabsList className="grid w-full grid-cols-5">
            <TabsTrigger value="experience">Experience</TabsTrigger>
            <TabsTrigger value="education">Education</TabsTrigger>
            <TabsTrigger value="projects">Projects</TabsTrigger>
            <TabsTrigger value="skills">Skills</TabsTrigger>
            <TabsTrigger value="languages">Languages</TabsTrigger>
          </TabsList>

          <TabsContent value="experience" className="space-y-6">
            {userProfile.workExperience.map((exp, index) => (
              <Card key={index} className="shadow-lg">
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-primary text-xl">
                        {exp.role}
                      </CardTitle>
                      <div className="text-muted-foreground mt-1 flex items-center gap-2">
                        <Building className="h-4 w-4" />
                        <span className="font-medium">{exp.company}</span>
                      </div>
                      <div className="text-muted-foreground mt-1 flex items-center gap-2 text-sm">
                        <Calendar className="h-4 w-4" />
                        <span>
                          {new Date(exp.startDate).toLocaleDateString()} -{" "}
                          {new Date(exp.endDate).toLocaleDateString()}
                        </span>
                      </div>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-2">
                    <p className="text-muted-foreground">
                      {exp.responsibilities}
                    </p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </TabsContent>

          <TabsContent value="education" className="space-y-6">
            {userProfile.education.map((edu, index) => (
              <Card key={index} className="shadow-lg">
                <CardContent className="p-6">
                  <div className="flex items-start gap-4">
                    <div className="bg-primary/10 rounded-full p-3">
                      <GraduationCap className="text-primary h-6 w-6" />
                    </div>
                    <div className="flex-1">
                      <h3 className="text-foreground text-xl font-semibold">
                        {edu.degree}
                      </h3>
                      <p className="text-primary font-medium">
                        {edu.fieldOfStudy}
                      </p>
                      <p className="text-muted-foreground">{edu.institution}</p>
                      <p className="text-muted-foreground mt-1 text-sm">
                        Graduated: {edu.graduationYear}
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </TabsContent>

          <TabsContent value="projects" className="space-y-6">
            <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
              {userProfile.projects.map((project, index) => (
                <Card key={index} className="shadow-lg">
                  <CardHeader>
                    <div className="flex items-start justify-between">
                      <CardTitle className="text-primary text-lg">
                        {project.title}
                      </CardTitle>
                      <Button variant="outline" size="sm" asChild>
                        <a
                          href={project.url}
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          <ExternalLink className="h-4 w-4" />
                        </a>
                      </Button>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <p className="text-muted-foreground mb-4">
                      {project.description}
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {project.technologies.map((tech, idx) => (
                        <Badge
                          key={idx}
                          variant="secondary"
                          className="bg-primary/10 text-primary"
                        >
                          {tech}
                        </Badge>
                      ))}
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </TabsContent>

          <TabsContent value="skills" className="space-y-6">
            <Card className="shadow-lg">
              <CardHeader>
                <CardTitle className="text-primary flex items-center gap-2">
                  <Code2 className="h-5 w-5" />
                  Programming Languages & Technologies
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {userProfile.programmingLanguages.map((lang, index) => (
                    <div
                      key={index}
                      className="bg-muted/50 flex items-center justify-between rounded-lg p-3"
                    >
                      <span className="text-foreground font-medium">
                        {lang.language}
                      </span>
                      <Badge
                        className={getProficiencyColor(lang.proficiency)}
                        variant="outline"
                      >
                        {lang.proficiency}
                      </Badge>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="languages" className="space-y-6">
            <Card className="shadow-lg">
              <CardHeader>
                <CardTitle className="text-primary flex items-center gap-2">
                  <Languages className="h-5 w-5" />
                  Speaking Languages
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {userProfile.speakingLanguages.map((lang, index) => (
                    <div
                      key={index}
                      className="bg-muted/50 flex items-center justify-between rounded-lg p-3"
                    >
                      <span className="text-foreground font-medium">
                        {lang.language}
                      </span>
                      <Badge
                        className={getProficiencyColor(lang.proficiency)}
                        variant="outline"
                      >
                        {lang.proficiency}
                      </Badge>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
