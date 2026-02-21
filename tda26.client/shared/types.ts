export type WebTheme = "dark" | "light";

export type CourseStatus = "draft" | "scheduled" | "live" | "paused" | "archived";

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
    }
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

export interface Account {
    username: string;
    uuid: string,
    fullName: string,
    fullNameWithoutTitles: string,
    likes: Rating[];
    dislikes: Rating[];
    type: "account" | "lecturer" | "admin"
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
    purpose: "createMaterial" | "updateMaterial" | "deleteMaterial" | "createQuiz" | "updateQuiz" | "deleteQuiz" | "default";
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
