import "./style.css";
import "./categoryDialog";
import "./EntryElement";
import { EntryEditor } from "./entryEditor";
import "./projectDialog";
import { Settings } from "./settings";
import type { EntryElement } from "./EntryElement";
import type { Entry } from "./models";
import { formatDateTime } from "./time";
import { DeleteDialog } from "./deleteDialog";
import { EndDialog } from "./endDialog";
import { createEntry, deleteEntry, getInit, updateEntry } from "./api";

declare const addButton: HTMLButtonElement;
declare const newButtons: HTMLDialogElement;
declare const active: HTMLDivElement;
declare const today: HTMLDivElement;
declare const timeWorked: HTMLHeadingElement;
declare const goHomeTime: HTMLHeadingElement;

declare const allCategories: HTMLDataListElement;
declare const project: HTMLSelectElement;
declare const locationInput: HTMLSelectElement;

let updateTimesId: number;

window.addEventListener(
    "load",
    async () => {
        document.addEventListener("click", e => {
            if (!(e.target instanceof HTMLDialogElement)) {
                return;
            }
            const box = e.target.getBoundingClientRect();
            if (box.left <= e.clientX && box.top <= e.clientY && e.clientX <= box.right && e.clientY <= box.bottom) {
                return;
            }
            e.target.close("close");
        });

        Settings.updateCallback = loadInit;
        await loadInit();

        addButton.addEventListener("click", createNewEntry);
        addButton.addEventListener(
            "touchstart",
            e => {
                const start = e.timeStamp;
                e.preventDefault();
                const timeout = 500;
                const ac = new AbortController();
                setTimeout(
                    () => {
                        if (ac.signal.aborted) {
                            return;
                        }
                        newButtons.showPopover({ source: addButton });
                    },
                    timeout,
                );
                addButton.addEventListener(
                    "touchmove",
                    () => ac.abort(),
                    {
                        once: true,
                        signal: ac.signal,
                    },
                );
                addButton.addEventListener(
                    "touchcancel",
                    () => ac.abort(),
                    {
                        once: true,
                        signal: ac.signal,
                    },
                );
                addButton.addEventListener(
                    "touchend",
                    e => {
                        if (e.timeStamp - start < timeout) {
                            createNewEntry();
                        } else {
                            newButtons.showPopover({ source: addButton });
                        }
                        e.preventDefault();
                        ac.abort();
                    },
                    {
                        once: true,
                        signal: ac.signal,
                    },
                );
            },
        );

        document.addEventListener(
            "delete",
            async e => {
                const deleteConfirmed = await DeleteDialog.delete();
                if (!deleteConfirmed) {
                    return;
                }
                await deleteEntry(e.entry.id);
                document.getElementById(e.entry.id)?.remove();
                updateTimes();
            }
        );
        document.addEventListener(
            "edit",
            async e => {
                const entryCreateRequest = await EntryEditor.openEdit(e.entry);
                if (entryCreateRequest == null) {
                    return;
                }
                const updatedEntry = await updateEntry(e.entry.id, entryCreateRequest);
                const element = document.getElementById(e.entry.id) as EntryElement;
                element.remove();
                element.entry = updatedEntry;
                insertEntryElement(element);
                updateTimes();
            }
        );
        document.addEventListener(
            "end",
            async e => {
                const end = await EndDialog.end();
                if (end == null) {
                    return;
                }
                const updatedEntry = await updateEntry(
                    e.entry.id,
                    {
                        start: e.entry.start,
                        end,
                        project: null,
                        categories: e.entry.categories,
                        location: e.entry.location?.id ?? null
                    }
                );
                const element = document.getElementById(e.entry.id) as EntryElement;
                element.remove();
                element.entry = updatedEntry;
                insertEntryElement(element);
                updateTimes();
            }
        );
    }
);

async function loadInit(): Promise<void> {
    allCategories.innerHTML = "";
    project.innerHTML = "";
    locationInput.innerHTML = "";
    for (const element of document.querySelectorAll("entry-element") as NodeListOf<EntryElement>) {
        element.remove();
    }
    try {
        const init = await getInit();
        if (init != null) {
            allCategories.innerHTML = init.categories.map(c => `<option value="${c}">${c}</option>`).join("");
            project.innerHTML = init.projects.map(p => `<option value="${p}">${p}</option>`).join("");
            locationInput.innerHTML = init.locations
                .map(l => `<option value="${l.id}">${l.name ?? l.address ?? l.id}</option>`)
                .join("");
            for (const entry of init.entries) {
                addEntryToUi(entry, false);
            }
        }
    } catch { }
    updateTimes();
}

async function createNewEntry(): Promise<void> {
    const entryCreateRequest = await EntryEditor.openNew()
    if (entryCreateRequest == null) {
        return;
    }
    const entry = await createEntry(entryCreateRequest);
    addEntryToUi(entry);
}

function addEntryToUi(entry: Entry, shouldUpdateTimes: boolean = true): void {
    const element = document.createElement("entry-element") as EntryElement;
    element.id = entry.id;
    element.entry = entry;
    insertEntryElement(element);
    if (shouldUpdateTimes) {
        updateTimes();
    }
}

function insertEntryElement(element: EntryElement): void {
    const parent = element.entry?.end == null ? active : today;
    const elements = parent.querySelectorAll("entry-element") as NodeListOf<EntryElement>;
    for (const other of elements) {
        if ((other.entry?.start.getTime() ?? 0) < (element.entry?.start.getTime() ?? 0)) {
            other.before(element);
            return;
        }
    }
    parent.appendChild(element);
}

function updateTimes(): void {
    const now = new Date();
    const startAndEndTimes = [...document.querySelectorAll("entry-element") as NodeListOf<EntryElement>]
        .map(e => e.entry)
        .filter(e => e != null)
        .filter(e => e.categories.includes("work"))
        .map(e => ({ start: e.start, end: e.end ?? now }));

    merge: do {
        for (let i = 0; i < startAndEndTimes.length; i++) {
            const a = startAndEndTimes[i];
            for (let j = i + 1; j < startAndEndTimes.length; j++) {
                const b = startAndEndTimes[j];
                if ((a.start <= b.start && b.start <= a.end) || (a.start <= b.end && b.end <= a.end)
                    || (b.start <= a.start && a.start <= b.end) || (b.start <= a.end && a.end <= b.end)) {
                    startAndEndTimes[i] = {
                        start: new Date(Math.min(a.start.getTime(), b.start.getTime())),
                        end: new Date(Math.max(a.end.getTime(), b.end.getTime())),
                    }
                    startAndEndTimes.splice(j, 1);
                    continue merge;
                }
            }
        }
    } while (false);

    const totalTime = startAndEndTimes.reduce((t, e) => t + (e.end.getTime() - e.start.getTime()), 0);
    const hours = Math.floor(totalTime / 1000 / 60 / 60).toString();
    const minutes = Math.round(totalTime / 1000 / 60 % 60).toString().padStart(2, "0");
    timeWorked.innerText = `${hours}h ${minutes}m`;

    goHomeTime.innerText = formatDateTime(new Date(now.getTime() + (8 * 60 * 60 * 1000 - totalTime)));

    clearTimeout(updateTimesId);
    updateTimesId = setTimeout(updateTimes, 60000);
}
