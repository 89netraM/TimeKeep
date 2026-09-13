import "./style.css";
import "./categoryDialog";
import "./EntryElement";
import { EntryEditor } from "./entryEditor";
import "./projectDialog";
import "./settings";
import type { EntryElement } from "./EntryElement";
import type { Entry } from "./models";
import { formatDateTime } from "./time";
import { DeleteDialog } from "./deleteDialog";
import { EndDialog } from "./endDialog";

declare const addButton: HTMLButtonElement;
declare const newButtons: HTMLDialogElement;
declare const active: HTMLDivElement;
declare const today: HTMLDivElement;
declare const timeWorked: HTMLHeadingElement;
declare const goHomeTime: HTMLHeadingElement;

let updateTimesId: number;

window.addEventListener(
    "load",
    () => {
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
                console.log("TODO: Make RPC deleting entry");
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
                console.log("TODO: Make RPC and edit entry");
                const element = document.getElementById(e.entry.id) as EntryElement;
                element.remove();
                // TODO: Update element.entry based on RPC call
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
                console.log("TODO: Make RPC ending entry");
                const element = document.getElementById(e.entry.id) as EntryElement;
                element.remove();
                element.entry = {
                    ...e.entry,
                    end,
                };
                insertEntryElement(element);
                updateTimes();
            }
        );

        const date = new Date().toISOString().substring(0, 10);
        addEntryToUi({
            id: "9c66ebb6-547c-4752-b302-a86712c3eef6",
            start: new Date(`${date}T11:00:00.000Z`),
            end: null,
            location: "home",
            categories: ["timekeep", "work"],
        });
        addEntryToUi({
            id: "5fd1c236-b5db-4d8d-803e-27f005047c20",
            start: new Date(`${date}T06:00:00.000Z`),
            end: new Date(`${date}T09:00:00.000Z`),
            location: "home",
            categories: ["timekeep", "work"],
        });
        addEntryToUi({
            id: "bc4b5f20-fae5-4376-812b-55a0fc431759",
            start: new Date(`${date}T06:45:00.000Z`),
            end: new Date(`${date}T07:00:00.000Z`),
            location: "home",
            categories: ["meeting", "timekeep", "work"],
        });

        updateTimes();
    }
);

async function createNewEntry(): Promise<void> {
    const entryCreateRequest = await EntryEditor.openNew()
    console.log({ entryCreateRequest });
}

function addEntryToUi(entry: Entry): void {
    const element = document.createElement("entry-element") as EntryElement;
    element.id = entry.id;
    element.entry = entry;
    insertEntryElement(element);
    updateTimes();
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
    const hours = Math.round(totalTime / 1000 / 60 / 60).toString();
    const minutes = Math.round(totalTime / 1000 / 60 % 60).toString().padStart(2, "0");
    timeWorked.innerText = `${hours}h ${minutes}m`;

    goHomeTime.innerText = formatDateTime(new Date(now.getTime() + (8 * 60 * 60 * 1000 - totalTime)));

    clearTimeout(updateTimesId);
    updateTimesId = setTimeout(updateTimes, 60000);
}
