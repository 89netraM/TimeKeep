export interface Entry {
    readonly id: string;
    readonly start: Date;
    readonly end: Date | null;
    readonly categories: ReadonlyArray<string>;
    readonly location: Location | null;
}

export interface Location {
    readonly id: string;
    readonly name: string | null;
    readonly address: string | null;
}

export interface Init {
    readonly categories: ReadonlyArray<string>;
    readonly entries: ReadonlyArray<Entry>;
    readonly locations: ReadonlyArray<Location>;
    readonly projects: ReadonlyArray<string>;
}

export interface EntryRequest {
    readonly start: Date;
    readonly end: Date | null;
    readonly project: string | null;
    readonly categories: ReadonlyArray<string>;
    readonly location: string | null;
}

export interface ProjectRequest {
    readonly project: string;
    readonly categories: ReadonlyArray<string>;
}

export interface CategoryRequest {
    readonly category: string;
}
