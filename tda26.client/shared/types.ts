export type WebTheme = "dark" | "light";

export interface Course {
    uuid: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
    materials: Material[];
    quizzes: Quiz[];
    feed: FeedPost[];
    tags: Tag[] | null;
}

export interface Lecturer {
    uuid: string,
    titleBefore: string | null,
    firstName: string,
    middleName: string | null,
    lastName: string,
    titleAfter: string | null,
    bio: string | null,
    pictureUrl: string | null,
    claim: string | null,
    pricePerHour: number,
    mobileNumbers: string[] | null,
    emails: string[] | null,
    tags: string[] | null,
    isPublic: boolean,
    location: string | null,
    createdAt: string,
    updatedAt: string,
}

export interface Account extends Lecturer{
    username: string,
}

export interface BaseMaterial {
    uuid: string;
    type: 'file' | 'url';
    courseUuid: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
}

export interface FileMaterial extends BaseMaterial {
    type: 'file';
    fileUrl: string;
    mimeType: string | null;
    sizeBytes: number;
}

export interface UrlMaterial extends BaseMaterial {
    type: 'url';
    url: string;
    faviconUrl: string | null;
}

export type Material = FileMaterial | UrlMaterial;

export interface Quiz {
    uuid: string,
    courseUuid: string,
    title: string,
    questionCount: string,
    createdAt: string,
    updatedAt: string,
}

export interface FeedPost {
    uuid: string,
    createdAt: string,
    updatedAt: string,
    courseUuid: string,
}
export interface Tag{
    uuid: string,
    displayName: string,
    createdAt: string,
    updatedAt: string,
}

export type ClassLike = string | undefined | Record<string, boolean | null | undefined> | ClassLike[];