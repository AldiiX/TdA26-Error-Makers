export type WebTheme = "dark" | "light";

export interface Course {
    uuid: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
    //lecturerUuid: string;
    lecturer: Lecturer | null;
    materials?: Material[];
    quizzes?: Quiz[];
    // feed: FeedPost[];
    tags: Tag[] | null;
    likeCount: number,
    viewCount: number,
}

export interface Material {
    uuid: string;
    name: string;
    type: number;
    url?: string;
    fileUrl?: string;
    createdAt: string;
    description?: string;
    faviconUrl?: string;
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
}

export interface Question {
    uuid?: string;
    question: string;
    options: string[];
    type: "singleChoice" | "multipleChoice";
    correctIndex?: number;
    correctIndices?: number[];
}

export interface CourseFormModel {
    name: string;
    description: string;
    materials: MaterialFormModel[];
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