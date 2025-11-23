export type WebTheme = "dark" | "light";

export interface Course {
    uuid: string;
    name: string;
    description: string;
    createdAt: string;
    updatedAt: string;
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
    mobileNumbers: string[],
    emails: string[],
    tags: string[],
    location: string | null,
    createdAt: string,
    updatedAt: string,
}

export interface Account extends Lecturer {
    username: string
}

export type ClassLike = string | undefined | Record<string, boolean | null | undefined> | ClassLike[];