"use client";

import {
  Briefcase,
  Code,
  Download,
  ExpandIcon,
  ExternalLink,
  FileText,
  FolderOpen,
  GraduationCap,
  Languages,
  Mail,
  MapPin,
  Phone,
  Play,
  Sparkles,
  Users,
} from "lucide-react";
import { Suspense, useEffect, useState } from "react";
import { useParams, useSearchParams } from "next/navigation";

import InterviewAudioPlayer from "@/components/InterviewAudioPlayer";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import BackButton from "@/components/ui/back-button";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Separator } from "@/components/ui/separator";

import { useApi } from "@/hooks/useApi";
import { interviewAudioService } from "@/lib/services/interviewAudioService";
import { downloadBlob, downloadFromSasUrl } from "@/lib/fileUtils";
import { profileService, type EducationDto, type ExperienceDto, type SkillDto } from "@/lib/api/services/profile.service";
import { useUnifiedAuth } from "@/hooks/useUnifiedAuth";
import { cn } from "@/lib/utils";

function DebriefContent() {
  const params = useParams();
  const searchParams = useSearchParams();
  const api = useApi();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [jobPost, setJobPost] = useState<any>(null);
  const [candidate, setCandidate] = useState<any>(null);
  const [audioData, setAudioData] = useState<any>(null);
  const [jobApplication, setJobApplication] = useState<any>(null);
  const [audioLoading, setAudioLoading] = useState(false);
  const [downloadingResume, setDownloadingResume] = useState(false);
  const [profileDetailsOpen, setProfileDetailsOpen] = useState(false);
  const [education, setEducation] = useState<EducationDto[]>([]);
  const [experience, setExperience] = useState<ExperienceDto[]>([]);
  const [projects, setProjects] = useState<any[]>([]);
  const [skills, setSkills] = useState<SkillDto[]>([]);
  const [loadingProfileDetails, setLoadingProfileDetails] = useState(false);
  const { getAccessToken } = useUnifiedAuth();

  useEffect(() => {
    async function fetchData() {
      const jobPostName = params.name as string;
      const jobPostVersionStr = params.version as string;
      const candidateId = searchParams.get("candidateId");

      if (!jobPostName || !jobPostVersionStr || !candidateId) {
        setError(`Invalid parameters: name=${jobPostName}, version=${jobPostVersionStr}, candidateId=${candidateId}`);
        setLoading(false);
        return;
      }

      const jobPostVersion = parseInt(jobPostVersionStr, 10);
      if (isNaN(jobPostVersion)) {
        setError(`Invalid job post version: ${jobPostVersionStr}`);
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        
        // Single API call to get job application with steps and interviews
        // This includes the job post data in JobApplication.JobPost
        const appData = await interviewAudioService.getJobApplicationWithInterviews(
          api,
          jobPostName,
          jobPostVersion,
          candidateId
        );

        // Extract job post from application data (if available) or fetch separately
        if (appData?.jobApplication?.jobPost) {
          setJobPost(appData.jobApplication.jobPost);
        } else {
          // Fallback: fetch job post if not included in response
          const jobPostResponse = await api.get(`/job/${jobPostName}/${jobPostVersion}`);
          setJobPost(jobPostResponse.data || jobPostResponse);
        }

        // Fetch candidate (required for user profile)
        const candidateResponse = await api.get(`/candidate/${candidateId}`);
        setCandidate(candidateResponse.data || candidateResponse);

        // Store audio data and job application for later use
        setAudioData(appData);
        setJobApplication(appData?.jobApplication || appData);

      } catch (err: any) {
        console.error("Error fetching debrief data:", err);
        setError(err?.message || "Failed to load debrief data");
      } finally {
        setLoading(false);
      }
    }

    const candidateId = searchParams.get("candidateId");
    if (params.name && params.version && candidateId) {
      fetchData();
    } else {
      setLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params.name, params.version, searchParams]);

  const loadAudio = async () => {
    // Audio data is already loaded from initial fetch, no need to fetch again
    if (audioData) return;
    
    // This should not happen since we load it in useEffect, but just in case
    const jobPostName = params.name as string;
    const jobPostVersionStr = params.version as string;
    const candidateId = searchParams.get("candidateId");

    if (!jobPostName || !jobPostVersionStr || !candidateId) return;

    const jobPostVersion = parseInt(jobPostVersionStr, 10);
    if (isNaN(jobPostVersion)) return;

    setAudioLoading(true);
    try {
      const data = await interviewAudioService.getJobApplicationWithInterviews(
        api,
        jobPostName,
        jobPostVersion,
        candidateId
      );
      setAudioData(data);
    } catch (error) {
      console.error('Failed to load audio:', error);
    } finally {
      setAudioLoading(false);
    }
  };

  const handleDownloadResume = async () => {
    const candidateId = searchParams.get("candidateId");
    if (!candidateId || downloadingResume || !candidate) return;

    const userProfile = candidate.userProfile || {};
    const name = userProfile.name || "Candidate";

    setDownloadingResume(true);
    try {
      // Get SAS URL from backend
      const response = await api.get(`/userprofile/candidate/${candidateId}/resume`);
      
      // Response format: { downloadUrl: string, expiresInMinutes: number }
      // apiClient returns data directly, not wrapped in .data
      const downloadUrl = (response as any).downloadUrl || (response as any).data?.downloadUrl;
      
      if (!downloadUrl) {
        console.error('Download response:', response);
        throw new Error('Download URL not received from server');
      }
      
      // Download directly from Azure using SAS URL
      await downloadFromSasUrl(downloadUrl, `${name}_Resume.pdf`);
    } catch (error) {
      console.error('Failed to download resume:', error);
    } finally {
      setDownloadingResume(false);
    }
  };

  const fetchProfileDetails = async () => {
    if (!candidate?.userProfile?.id || loadingProfileDetails) return;

    setLoadingProfileDetails(true);
    try {
      const token = await getAccessToken();
      if (!token) return;

      const userProfileId = candidate.userProfile.id;
      
      // Use admin endpoints that accept userProfileId parameter for viewing candidate data
      const [educationData, experienceData, projectsData, skillsData] = await Promise.all([
        profileService.getEducationByUserProfileId(token, userProfileId),
        profileService.getExperienceByUserProfileId(token, userProfileId),
        api.get(`/projectresearch/user-profile/${userProfileId}`).then(res => res.data || res || []).catch(() => []),
        profileService.getSkillsByUserProfileId(token, userProfileId)
      ]);

      setEducation(educationData || []);
      setExperience(experienceData || []);
      setProjects(Array.isArray(projectsData) ? projectsData : []);
      setSkills(skillsData || []);
    } catch (error) {
      console.error('Failed to fetch profile details:', error);
    } finally {
      setLoadingProfileDetails(false);
    }
  };

  const handleProfileDetailsOpenChange = (open: boolean) => {
    setProfileDetailsOpen(open);
    if (open) {
      fetchProfileDetails();
    }
  };

  if (loading) {
    return (
      <div className="container p-6">
        <div className="text-center">
          <p className="text-muted-foreground">Loading debrief...</p>
        </div>
      </div>
    );
  }

  if (error || !jobPost || !candidate) {
    return (
      <div className="container p-6">
        <div className="text-center">
          <h1 className="mb-4 text-2xl font-bold">Debrief Not Found</h1>
          <p className="text-muted-foreground mb-4">{error || "Unable to load debrief data"}</p>
          <BackButton />
        </div>
      </div>
    );
  }

  const userProfile = candidate.userProfile || {};
  const candidateName = userProfile.name || "Candidate";
  const playlist = audioData ? interviewAudioService.createPlaylist(audioData, candidateName) : [];

  const getScoreColor = (score: number) => {
    // Score is on a 0-10 scale, so we convert thresholds accordingly
    // 85/100 = 8.5/10, 70/100 = 7.0/10
    if (score >= 7) return "bg-green-100 text-green-800";
    if (score >= 5) return "bg-yellow-100 text-yellow-800";
    return "bg-red-100 text-red-800";
  };

  // Aggregate scorings: max per fixedCategory, average across unique categories
  const aggregateScores = () => {
    const scorings: any[] = candidate.scorings || [];
    const grouped = new Map<string, number>();
    scorings.forEach((s) => {
      const label = (s.fixedCategory || s.category || "Other").toString().trim();
      const val = typeof s.score === "number" ? s.score : null;
      if (val == null) return;
      const current = grouped.get(label);
      grouped.set(label, current == null ? val : Math.max(current, val));
    });

    const numericScores = Array.from(grouped.values());
    const avg =
      numericScores.length > 0
        ? Math.round(
            numericScores.reduce((a, b) => a + b, 0) / numericScores.length,
          )
        : 0;

    return { avg };
  };

  const { avg: overallScore } = aggregateScores();

  const getProficiencyColor = (proficiency: string) => {
    switch (proficiency) {
      case "advanced":
        return "bg-green-100 text-green-800";
      case "intermediate":
        return "bg-yellow-100 text-yellow-800";
      case "beginner":
        return "bg-red-100 text-red-800";
      default:
        return "bg-gray-100 text-gray-800";
    }
  };

  return (
    <div className="container space-y-7 p-6">
      <BackButton />

      <div>
        <h1 className="mb-2 text-3xl font-bold">Interview Debrief</h1>
        <p className="text-muted-foreground">
          {jobPost.jobTitle} • {candidateName}
        </p>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Candidate Info Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              Candidate Information
            </CardTitle>
            <CardDescription>Profile and background details</CardDescription>
          </CardHeader>

          <CardContent className="space-y-4">
            <div className="flex items-center gap-4">
              <Avatar className="h-16 w-16">
                <AvatarImage
                  src={userProfile.profilePictureUrl}
                  alt={candidateName}
                />
                <AvatarFallback>
                  {candidateName
                    .split(" ")
                    .map((n: string) => n[0])
                    .join("")}
                </AvatarFallback>
              </Avatar>

              <div>
                <h3 className="text-xl font-semibold">{candidateName}</h3>
                {userProfile.age && (
                  <p className="text-muted-foreground">
                    {userProfile.age} years old
                  </p>
                )}
                <Badge className={getScoreColor(overallScore)}>
                  Overall Score: {overallScore}/10
                </Badge>
              </div>
            </div>

            <div className="space-y-2">
              {userProfile.nationality && (
                <div className="flex items-center gap-2 text-sm">
                  <MapPin className="text-muted-foreground h-4 w-4" />
                  <span>{userProfile.nationality}</span>
                </div>
              )}

              {userProfile.email && (
                <div className="flex items-center gap-2 text-sm">
                  <Mail className="text-muted-foreground h-4 w-4" />
                  <span>{userProfile.email}</span>
                </div>
              )}

              {userProfile.phoneNumber && (
                <div className="flex items-center gap-2 text-sm">
                  <Phone className="text-muted-foreground h-4 w-4" />
                  <span>{userProfile.phoneNumber}</span>
                </div>
              )}
            </div>

            {userProfile.bio && (
              <p className="text-muted-foreground text-sm">{userProfile.bio}</p>
            )}
          </CardContent>

          <CardFooter className="flex gap-2">
            {(userProfile.resumeUrl || candidate.cvFile?.filePath) && (
              <Button
                variant="outline"
                size="sm"
                onClick={handleDownloadResume}
                disabled={downloadingResume}
                isLoading={downloadingResume}
              >
                <Download className="mr-2 h-4 w-4" />
                Download Resume
              </Button>
            )}

            <Dialog open={profileDetailsOpen} onOpenChange={handleProfileDetailsOpenChange}>
              <DialogTrigger asChild>
                <Button variant="outline" size="sm">
                  <ExpandIcon className="mr-2 h-4 w-4" />
                  View Details
                </Button>
              </DialogTrigger>

              <DialogContent className="max-h-[80vh] overflow-y-auto sm:max-w-4xl">
                <DialogHeader>
                  <DialogTitle>{candidateName} - Detailed Profile</DialogTitle>
                  <DialogDescription>
                    Complete candidate information and background
                  </DialogDescription>
                </DialogHeader>

                {loadingProfileDetails ? (
                  <div className="flex items-center justify-center py-8">
                    <div className="text-center">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto mb-2"></div>
                      <p className="text-muted-foreground text-sm">Loading profile details...</p>
                    </div>
                  </div>
                ) : (
                  <div className="space-y-6">
                    {/* Education */}
                    <div>
                      <h4 className="mb-3 flex items-center gap-2 font-semibold">
                        <GraduationCap className="h-4 w-4" />
                        Education
                      </h4>
                      <div className="space-y-3">
                        {education.length > 0 ? (
                          education.map((edu) => (
                            <div key={edu.id} className="rounded-lg border p-3">
                              <div className="flex items-start justify-between">
                                <div className="flex-1">
                                  {edu.degree && (
                                    <p className="font-medium text-sm">{edu.degree}</p>
                                  )}
                                  {edu.institution && (
                                    <p className="text-muted-foreground text-sm">{edu.institution}</p>
                                  )}
                                  {edu.fieldOfStudy && (
                                    <p className="text-muted-foreground text-sm">{edu.fieldOfStudy}</p>
                                  )}
                                  {(edu.startDate || edu.endDate) && (
                                    <p className="text-muted-foreground text-xs mt-1">
                                      {edu.startDate ? new Date(edu.startDate).getFullYear() : ''}
                                      {edu.startDate && edu.endDate ? ' - ' : ''}
                                      {edu.endDate ? new Date(edu.endDate).getFullYear() : ''}
                                    </p>
                                  )}
                                  {edu.location && (
                                    <p className="text-muted-foreground text-xs">{edu.location}</p>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))
                        ) : (
                          <div className="rounded-lg border p-3">
                            <p className="text-muted-foreground text-sm">No education details available</p>
                          </div>
                        )}
                      </div>
                    </div>

                    <Separator />

                    {/* Work Experience */}
                    <div>
                      <h4 className="mb-3 flex items-center gap-2 font-semibold">
                        <Briefcase className="h-4 w-4" />
                        Work Experience
                      </h4>
                      <div className="space-y-3">
                        {experience.length > 0 ? (
                          experience.map((exp) => (
                            <div key={exp.id} className="rounded-lg border p-3">
                              <div className="flex items-start justify-between">
                                <div className="flex-1">
                                  {exp.title && (
                                    <p className="font-medium text-sm">{exp.title}</p>
                                  )}
                                  {exp.organization && (
                                    <p className="text-muted-foreground text-sm">{exp.organization}</p>
                                  )}
                                  {(exp.startDate || exp.endDate) && (
                                    <p className="text-muted-foreground text-xs mt-1">
                                      {exp.startDate ? new Date(exp.startDate).toLocaleDateString() : ''}
                                      {exp.startDate && exp.endDate ? ' - ' : ''}
                                      {exp.endDate ? new Date(exp.endDate).toLocaleDateString() : 'Present'}
                                    </p>
                                  )}
                                  {exp.location && (
                                    <p className="text-muted-foreground text-xs">{exp.location}</p>
                                  )}
                                  {exp.description && (
                                    <p className="text-muted-foreground text-sm mt-2">{exp.description}</p>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))
                        ) : (
                          <div className="rounded-lg border p-3">
                            <p className="text-muted-foreground text-sm">No work experience details available</p>
                          </div>
                        )}
                      </div>
                    </div>

                    <Separator />

                    {/* Projects */}
                    <div>
                      <h4 className="mb-3 flex items-center gap-2 font-semibold">
                        <FolderOpen className="h-4 w-4" />
                        Projects
                      </h4>
                      <div className="space-y-3">
                        {projects.length > 0 ? (
                          projects.map((project: any) => (
                            <div key={project.id} className="rounded-lg border p-3">
                              <div className="flex items-start justify-between">
                                <div className="flex-1">
                                  {project.title && (
                                    <p className="font-medium text-sm">{project.title}</p>
                                  )}
                                  {project.description && (
                                    <p className="text-muted-foreground text-sm mt-1">{project.description}</p>
                                  )}
                                  {project.url && (
                                    <a 
                                      href={project.url} 
                                      target="_blank" 
                                      rel="noopener noreferrer"
                                      className="text-primary text-xs mt-1 hover:underline inline-flex items-center gap-1"
                                    >
                                      View Project
                                      <ExternalLink className="h-3 w-3" />
                                    </a>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))
                        ) : (
                          <div className="rounded-lg border p-3">
                            <p className="text-muted-foreground text-sm">No project details available</p>
                          </div>
                        )}
                      </div>
                    </div>

                    <Separator />

                    {/* Skills */}
                    <div>
                      <h4 className="mb-3 flex items-center gap-2 font-semibold">
                        <Sparkles className="h-4 w-4" />
                        Skills
                      </h4>
                      <div className="grid grid-cols-2 gap-3">
                        {skills.length > 0 ? (
                          skills.map((skill) => (
                            <div key={skill.id} className="rounded-lg border p-3">
                              <div className="flex items-center justify-between">
                                <div className="flex-1 min-w-0">
                                  <p className="font-medium text-sm truncate">{skill.skillName}</p>
                                  {skill.category && (
                                    <p className="text-muted-foreground text-xs truncate">{skill.category}</p>
                                  )}
                                </div>
                                {skill.proficiency && (
                                  <Badge className={cn(getProficiencyColor(skill.proficiency), "ml-2 shrink-0")} variant="outline">
                                    {skill.proficiency}
                                  </Badge>
                                )}
                              </div>
                              {skill.yearsExperience && (
                                <p className="text-muted-foreground text-xs mt-1">
                                  {skill.yearsExperience} {skill.unit || "years"} experience
                                </p>
                              )}
                            </div>
                          ))
                        ) : (
                          <div className="col-span-2 rounded-lg border p-3">
                            <p className="text-muted-foreground text-sm">No skills available</p>
                          </div>
                        )}
                      </div>
                    </div>

                    <Separator />

                    {/* Languages */}
                    <div>
                      <h4 className="mb-3 flex items-center gap-2 font-semibold">
                        <Languages className="h-4 w-4" />
                        Languages
                      </h4>
                      <div className="grid grid-cols-2 gap-3">
                        {candidate?.userProfile?.speakingLanguages && candidate.userProfile.speakingLanguages.length > 0 ? (
                          candidate.userProfile.speakingLanguages.map((lang: any, index: number) => (
                            <div key={index} className="rounded-lg border p-3">
                              <div className="flex items-center justify-between">
                                <span className="font-medium text-sm truncate">{lang.language || lang.name || "Unknown"}</span>
                                {lang.proficiency && (
                                  <Badge className={cn(getProficiencyColor(lang.proficiency), "ml-2 shrink-0")} variant="outline">
                                    {lang.proficiency}
                                  </Badge>
                                )}
                              </div>
                            </div>
                          ))
                        ) : (
                          <div className="col-span-2 rounded-lg border p-3">
                            <p className="text-muted-foreground text-sm">No language details available</p>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                )}
              </DialogContent>
            </Dialog>
          </CardFooter>
        </Card>

        {/* Job Info Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Briefcase className="h-5 w-5" />
              Job Information
            </CardTitle>
            <CardDescription>Position details and requirements</CardDescription>
          </CardHeader>

          <CardContent className="space-y-4">
            <div>
              <h3 className="text-xl font-semibold">{jobPost.jobTitle}</h3>
              <div className="mt-2 flex gap-2">
                {jobPost.jobType && (
                  <Badge variant="outline">{jobPost.jobType}</Badge>
                )}
                {jobPost.experienceLevel && (
                  <Badge variant="outline">{jobPost.experienceLevel}</Badge>
                )}
                {jobPost.tone && (
                  <Badge variant="outline">{jobPost.tone}</Badge>
                )}
              </div>
            </div>

            <div className="space-y-2">
              {jobPost.focusArea && (
                <div className="text-sm">
                  <span className="font-medium">Focus Area:</span> {jobPost.focusArea}
                </div>
              )}
              {jobPost.probingDepth && (
                <div className="text-sm">
                  <span className="font-medium">Probing Depth:</span> {jobPost.probingDepth}
                </div>
              )}
              {jobPost.candidatesCount !== undefined && (
                <div className="text-sm">
                  <span className="font-medium">Candidates:</span> {jobPost.candidatesCount}
                </div>
              )}
            </div>

            {jobPost.jobDescription && (
              <p className="text-muted-foreground line-clamp-4 text-sm">
                {jobPost.jobDescription}
              </p>
            )}
          </CardContent>

          <CardFooter className="flex gap-2">
            <Dialog>
              <DialogTrigger asChild>
                <Button variant="outline" size="sm">
                  <ExpandIcon className="mr-2 h-4 w-4" />
                  View Details
                </Button>
              </DialogTrigger>

              <DialogContent className="max-h-[80vh] overflow-y-auto sm:max-w-4xl">
                <DialogHeader>
                  <DialogTitle>{jobPost.jobTitle} - Job Details</DialogTitle>
                  <DialogDescription>Complete job posting information</DialogDescription>
                </DialogHeader>

                <div className="space-y-6">
                  {jobPost.jobDescription && (
                    <div>
                      <h4 className="mb-2 font-semibold">Job Description</h4>
                      <p className="text-muted-foreground text-sm">{jobPost.jobDescription}</p>
                    </div>
                  )}

                  {(jobPost.minimumRequirements?.length > 0) && (
                    <>
                      <Separator />
                      <div>
                        <h4 className="mb-2 font-semibold">Minimum Requirements</h4>
                        <ul className="list-inside list-disc space-y-1 text-sm">
                          {jobPost.minimumRequirements.map((req: string, index: number) => (
                            <li key={index}>{req}</li>
                          ))}
                        </ul>
                      </div>
                    </>
                  )}

                  <Separator />

                  {audioData?.steps && audioData.steps.length > 0 && (
                    <>
                      <div>
                        <h4 className="mb-3 font-semibold">Application Steps</h4>
                        <div className="space-y-3">
                          {audioData.steps.map((stepData: any, index: number) => {
                            const step = stepData.step;
                            const interviews = stepData.interviews || [];
                            const hasInterviews = interviews.length > 0;
                            
                            return (
                              <div key={step?.id || index} className="border rounded-lg p-3 bg-muted/20">
                                <div className="flex items-start justify-between">
                                  <div className="flex-1">
                                    <div className="flex items-center gap-2 mb-1">
                                      <span className="font-medium text-sm">Step {step?.stepNumber || index + 1}:</span>
                                      <span className="text-sm">{step?.jobPostStepName || "Unknown Step"}</span>
                                    </div>
                                    <div className="flex items-center gap-3 text-xs text-muted-foreground">
                                      <span className={`px-2 py-1 rounded ${
                                        step?.status === "completed" ? "bg-green-100 text-green-800" :
                                        step?.status === "in_progress" ? "bg-blue-100 text-blue-800" :
                                        step?.status === "failed" ? "bg-red-100 text-red-800" :
                                        "bg-gray-100 text-gray-800"
                                      }`}>
                                        {step?.status || "pending"}
                                      </span>
                                      {hasInterviews && (
                                        <span className="flex items-center gap-1">
                                          <Play className="h-3 w-3" />
                                          {interviews.length} interview{interviews.length !== 1 ? "s" : ""}
                                        </span>
                                      )}
                                    </div>
                                  </div>
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      </div>
                      <Separator />
                    </>
                  )}

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <h4 className="mb-2 font-semibold">Additional Info</h4>
                      <div className="space-y-1 text-sm">
                        <p>
                          <span className="font-medium">Police Report Required:</span>{" "}
                          {jobPost.policeReportRequired ? "Yes" : "No"}
                        </p>
                        {jobPost.createdAt && (
                          <p>
                            <span className="font-medium">Created:</span>{" "}
                            {new Date(jobPost.createdAt).toLocaleDateString()}
                          </p>
                        )}
                      </div>
                    </div>
                  </div>

                  {jobPost.instructions && (
                    <>
                      <Separator />
                      <div>
                        <h4 className="mb-2 font-semibold">Special Instructions</h4>
                        <p className="text-muted-foreground text-sm">{jobPost.instructions}</p>
                      </div>
                    </>
                  )}
                </div>
              </DialogContent>
            </Dialog>
          </CardFooter>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Interview Questions - Static for now */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5" />
              Interview Questions
            </CardTitle>
            <CardDescription>Total questions asked during the interview</CardDescription>
          </CardHeader>

          <CardContent>
            <div className="text-center">
              <div className="text-primary mb-2 text-4xl font-bold">0</div>
              <p className="text-muted-foreground">Questions Asked</p>
            </div>
          </CardContent>
        </Card>

        {/* Interview Audio */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Play className="h-5 w-5" />
              Interview Audio
            </CardTitle>
            <CardDescription>Listen to the recorded interview session</CardDescription>
          </CardHeader>

          <CardContent>
            <Dialog>
              <DialogTrigger asChild>
                <Button 
                  className="w-full"
                  onClick={loadAudio}
                  disabled={audioLoading}
                >
                  <Play className="mr-2 h-4 w-4" />
                  {audioLoading ? "Loading..." : "Play Interview Audio"}
                </Button>
              </DialogTrigger>

              <DialogContent className="sm:max-w-4xl max-h-[80vh] overflow-y-auto">
                <DialogHeader>
                  <DialogTitle>Interview Audio Player</DialogTitle>
                </DialogHeader>

                {audioLoading ? (
                  <div className="text-center py-8">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
                    <p className="mt-2 text-muted-foreground">Loading...</p>
                  </div>
                ) : (
                  <InterviewAudioPlayer playlist={playlist} candidateName={candidateName} />
                )}
              </DialogContent>
            </Dialog>
          </CardContent>
        </Card>
      </div>

      <div className="grid h-44 place-content-center rounded-md border text-center">
        <p className="text-muted-foreground">
          Statements graph here (to be implemented)
        </p>
      </div>

      <div className="grid h-44 place-content-center rounded-md border text-center">
        <p className="text-muted-foreground">
          Stuttering graph here (to be implemented)
        </p>
      </div>
    </div>
  );
}

export default function Page() {
  return (
    <Suspense fallback={
      <div className="container p-6">
        <div className="text-center">
          <p className="text-muted-foreground">Loading debrief...</p>
        </div>
      </div>
    }>
      <DebriefContent />
    </Suspense>
  );
}

