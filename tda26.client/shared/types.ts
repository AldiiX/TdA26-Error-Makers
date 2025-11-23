export type WebTheme = "dark" | "light";

export interface Course {
    uuid: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
    materials: Materials[];
    quizzes: Quiz[];
    feed: FeedPost[];
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

export interface Materials {
    uuid: string,
    type: MaterialType,
    courseUuid: string,
    name: string,
    description: string,
    fileUrl: string,
    createdAt: string,
    updatedAt: string,
}

export enum MaterialType {
    VIDEO = "video",
    DOCUMENT = "document",
    AUDIO = "audio",
    IMAGE = "image"
}

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

export type ClassLike = string | undefined | Record<string, boolean | null | undefined> | ClassLike[];