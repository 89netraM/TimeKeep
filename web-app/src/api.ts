import type { CategoryRequest, Entry, EntryRequest, Init, ProjectRequest } from "./models";
import { Settings } from "./settings";

export async function getInit(): Promise<Init | null> {
    const token = Settings.token;
    if (token == null) {
        return null;
    }
    const response = await fetch(
        "/api/init",
        {
            method: "GET",
            headers: defaultHeaders(token),
        },
    );
    if (!response.ok) {
        return await raiseError("Init", response);
    }
    return await parse(response);
}

export async function createEntry(entry: EntryRequest): Promise<Entry> {
    const token = Settings.token;
    if (token == null) {
        return await raiseError("Create entry", "No token");
    }
    const response = await fetch(
        "/api/entry",
        {
            method: "POST",
            headers: defaultHeaders(token),
            body: JSON.stringify(entry),
        },
    );
    if (!response.ok) {
        return await raiseError("Create entry", response);
    }
    return await parse(response);
}

export async function updateEntry(id: string, entry: EntryRequest): Promise<Entry> {
    const token = Settings.token;
    if (token == null) {
        return await raiseError("Update entry", "No token");
    }
    const response = await fetch(
        `/api/entry/${id}`,
        {
            method: "PATCH",
            headers: defaultHeaders(token),
            body: JSON.stringify(entry),
        },
    );
    if (!response.ok) {
        return await raiseError("Update entry", response);
    }
    return await parse(response);
}

export async function deleteEntry(id: string): Promise<void> {
    const token = Settings.token;
    if (token == null) {
        return await raiseError("Update entry", "No token");
    }
    const response = await fetch(
        `/api/entry/${id}`,
        {
            method: "DELETE",
            headers: defaultHeaders(token),
        },
    );
    if (!response.ok) {
        return await raiseError("Update entry", response);
    }
}

export async function createProject(project: ProjectRequest): Promise<void> {
    const token = Settings.token;
    if (token == null) {
        return await raiseError("Create project", "No token");
    }
    const response = await fetch(
        "/api/project",
        {
            method: "POST",
            headers: defaultHeaders(token),
            body: JSON.stringify(project),
        },
    );
    if (!response.ok) {
        return await raiseError("Create project", response);
    }
}

export async function createCategory(category: CategoryRequest): Promise<void> {
    const token = Settings.token;
    if (token == null) {
        return await raiseError("Create category", "No token");
    }
    const response = await fetch(
        "/api/category",
        {
            method: "POST",
            headers: defaultHeaders(token),
            body: JSON.stringify(category),
        },
    );
    if (!response.ok) {
        return await raiseError("Create category", response);
    }
}

function defaultHeaders(token: string): HeadersInit {
    return {
        "Accepts": "application/json",
        "Authorization": `Bearer ${btoa(token)}`,
        "Content-Type": "application/json",
    };
}

async function raiseError(source: string, message: string | Response): Promise<never> {
    if (typeof message === "string") {
        throw new Error(`${source} error: ${message}`);
    } else if (message instanceof Response) {
        let remoteMessage;
        try {
            remoteMessage = await message.text();
        } catch (error) {
            throw new Error(`${source} error: ${error instanceof Error ? error.message : error}`);
        }
        if (remoteMessage != null && remoteMessage != "") {
            throw new Error(`${source} error: ${remoteMessage}`);
        }
            throw new Error(`${source} error: ${message.statusText}`);
    }
    throw new Error(`${source} error: Unknown`);
}

async function parse<T>(response: Response): Promise<T> {
    const json = await response.text();
    return JSON.parse(json, reviver);
}

const dateTimeRegex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z$/;
function reviver(_key: string, value: any): any {
    if (typeof value === "string" && dateTimeRegex.test(value)) {
        return new Date(value);
    }

    return value;
}
