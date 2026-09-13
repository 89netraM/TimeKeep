export interface Entry {
    readonly id: string;
    readonly start: Date;
    readonly end: Date | null;
    readonly categories: ReadonlyArray<string>;
    readonly location: Location | null;
}

export interface EntryCreateRequest {
    readonly start: Date;
    readonly end: Date | null;
    readonly project: string | null;
    readonly categories: ReadonlyArray<string>;
    readonly location: string | null;
}

export interface Location {
    readonly id: string;
    readonly name: string | null;
    readonly address: string | null;
}
