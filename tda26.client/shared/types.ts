export type WebTheme = "dark" | "light";

export interface Course {
    uuid: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
    // quizzes: Quiz[];
    materials?: Material[];
    // feed: FeedPost[];
    tags: Tag[] | null;
    lecturer: Lecturer | null;
    likeCount: number,
    viewCount: number,
    imageUrlOrDefault: string,
    image: string | null,
    category: {
        uuid: string,
        label: string,
    }
}

export interface Material {
    uuid: string;
    name: string;
    type: "url" | "file";
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