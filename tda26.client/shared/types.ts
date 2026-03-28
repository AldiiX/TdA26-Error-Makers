export type WebTheme = "dark" | "light";

export type CourseStatus = "draft" | "scheduled" | "live" | "paused" | "archived";

export interface Module {
    uuid: string;
    title: string;
    description?: string;
    isVisible: boolean;
    order: number;
    createdAt: string;
    updatedAt: string;
    materials: Material[];
    quizzes: Quiz[];
}

export interface Course {
    uuid: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
    status: CourseStatus;
    //lecturerUuid: string;
    lecturer: Lecturer | null;
    account: Account | null;
    materials?: Material[];
    quizzes?: Quiz[];
    modules?: Module[];
    feed?: FeedPost[];
    tags: Tag[] | null;
    likeCount: number,
    viewCount: number,
    imageUrl: string | null,
    categoryImageUrl: string,
    ratingScore: number,
    category: {
        uuid: string,
        label: string,
    },
    scheduledStart?: string,
}

export interface Material {
    uuid: string;
    name: string;
    type: "file" | "url";
    url?: string;
    fileUrl?: string;
    createdAt: string;
    description?: string;
    faviconUrl?: string;
    isVisible: boolean;
    order: number;
}

export interface MaterialFormModel {
    uuid?: string | null;
    name: string;
    type: "file" | "url";
    url?: string | null;
    file?: File | null;
    description?: string | null;
}

export interface Quiz {
    uuid: string;
    title: string;
    createdAt: string;
    updatedAt: string;
    attemptsCount: number;
    course: Course;
    questions: Question[];
    isVisible: boolean;
    order: number;
}

export interface Question {
    uuid?: string;
    question: string;
    options: string[];
    type: "singleChoice" | "multipleChoice";
    correctIndex?: number;
    correctIndices?: number[];
    selectedIndices?: number[];
    isCorrect?: boolean;
}

export interface QuizResult {
    uuid: string;
    quiz: Quiz;
    score: number;
    totalQuestions: number;
    completedAt: string;
}

export interface CourseFormModel {
    name: string;
    description: string;
    materials: MaterialFormModel[];
    categoryUuid: string;
    tagsUuid: string[];
}

export interface Lecturer extends Account {
    titleBefore: string | null,
    firstName: string,
    middleName: string | null,
    lastName: string,
    titleAfter: string | null,
    bio: string | null,
    pictureUrl: string | null,
    claim: string | null,
    pricePerHour: number,
    mobileNumbers: string[],
    emails: string[],
    tags: string[],
    location: string | null,
    createdAt: string,
    updatedAt: string,
}

export type AccountType = "account" | "lecturer" | "admin" | "student";

export function normalizeAccountType(type: string | null | undefined): AccountType {
    switch ((type ?? "").toLowerCase()) {
        case "lecturer":
            return "lecturer";
        case "admin":
            return "admin";
        case "student":
            return "student";
        default:
            return "account";
    }
}

export interface Account {
    username: string;
    uuid: string,
    fullName: string,
    fullNameWithoutTitles: string,
    likes: Rating[];
    dislikes: Rating[];
    type: AccountType;
    isPremium: boolean;
    dailyRewardXp?: number;
    dailyRewardDucks?: number;
}

interface Rating {
    course: Course | null,
    uuid: string
}

export interface gRecaptcha {
    ready: (callback: () => void) => void;
    execute: (siteKey: string, opts: { action: "submit" }) => Promise<string>;
}
export interface Tag{
    uuid: string,
    displayName: string,
    createdAt: string,
    updatedAt: string,
}

export type ClassLike = string | undefined | Record<string, boolean | null | undefined> | ClassLike[];

export interface AnswerSubmission {
    uuid: string;
    selectedIndex?: number;
    selectedIndices?: number[];
}

export interface FeedPost {
    uuid: string;
    type: "manual" | "system";
    message: string;
    edited: boolean;
    author: Author | null;
    createdAt: string;
    updatedAt: string;
    purpose: "createMaterial"
        | "updateMaterial"
        | "deleteMaterial"
        | "showMaterial"
        | "hideMaterial"
        | "createQuiz"
        | "updateQuiz"
        | "deleteQuiz"
        | "showQuiz"
        | "hideQuiz"
        | "hideModule"
        | "showModule"
        | "default";
}

export interface FeedPostView extends FeedPost {
    purposeLabel: string;
    purposeType: FeedPurposeType;
    icon: string;
    color: string;
    background: string;
}

export type FeedPurposeType = "announcement" | "material" | "quiz";

export interface Author {
    uuid: string;
    username: string;
    pictureUrl: string | null;
    titleBefore: string | null;
    firstName: string;
    middleName: string | null;
    lastName: string;
    titleAfter: string | null;
    bio: string | null,
    fullName: string,
    fullNameWithoutTitles: string,
}

export interface CourseCategory { 
    uuid: string;
    label: string;
}

export type TimeOption = {
    label: string;
    values: number | "custom";
};

export interface QuizResultsSummary {
    totalAttempts: number;
    averageScore: number;
    averageTimeSpent: number;
    averageScorePercentage: number;
    scoreDistribution: { 
        label: string; 
        count: number 
    }[];
    timeDistribution?: { 
        label: string; 
        count: number 
    }[];
}
export type CourseModule = Material | Quiz;

export interface DailyRewardTask {
    taskCode: string;
    title: string;
    description: string;
    currentValue: number;
    targetValue: number;
    isCompleted: boolean;
    completedAt: string | null;
    rewardXp: number;
    rewardDuck: number;
}

export interface DailyRewardDay {
    date: string;
    isClaimed: boolean;
    claimedAt: string | null;
    canClaim: boolean;
    isCompleted: boolean;
    tasks: DailyRewardTask[];
}

export interface DailyRewardsMonthResponse {
    year: number;
    month: number;
    daysInMonth: number;
    totalXp: number;
    totalDucks: number;
    days: DailyRewardDay[];
}


export interface UrlMaterialStats {
    materialUuid: string;
    type: "url";
    clickCount: number;
    lastClickedAt: string | null;
}

export interface FileMaterialStats {
    materialUuid: string;
    type: "file";
    sizeBytes: number;
    downloadCount: number;
    lastDownloadedAt: string | null;
    totalBytesDownloaded: number;
    totalMegabytesDownloaded: number;
    averageMegabytesPerDownload: number;
}

export type MaterialResultsSummary = UrlMaterialStats | FileMaterialStats;