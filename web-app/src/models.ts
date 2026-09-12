export interface Entry {
    readonly id: string;
    readonly start: Date;
    readonly end: Date | null;
    readonly categories: ReadonlySet<string>;
    readonly location: string | null;
}

export interface EntryCreateRequest {
    readonly start: Date;
    readonly end: Date | null;
    readonly project: string | null;
    readonly categories: ReadonlySet<string>;
    readonly location: string | null;
}
