import type { Entry, EntryCreateRequest } from "./models";

declare const entryDialog: HTMLDialogElement;
declare const entryFormReason: HTMLSpanElement;
declare const start: HTMLInputElement;
declare const end: HTMLInputElement;
declare const project: HTMLSelectElement;
declare const categories: HTMLSpanElement;
declare const categoryInput: HTMLInputElement;
declare const addCategoryButton: HTMLButtonElement;
declare const allCategories: HTMLDataListElement;
declare const location: HTMLInputElement;

window.addEventListener(
    "load",
    () => {
        categoryInput.addEventListener(
            "keydown",
            e => {
                if (e.key !== "Enter") {
                    return;
                }

                e.preventDefault();
                addCategory();
            }
        );
        addCategoryButton.addEventListener(
            "click",
            e => {
                e.preventDefault();
                addCategory();
            }
        );
        categories.addEventListener(
            "click",
            e => {
                if (!(e.target instanceof HTMLElement) || e.target.parentElement != categories) {
                    return;
                }

                e.target.remove();
            }
        );
    }
);

function addCategory() {
    if (categoryInput.value.length <= 0) {
        return;
    }

    categories.innerHTML += `<span>${categoryInput.value}</span>`;
    categoryInput.value = "";
}

function open(reason: string, input: Entry | null): Promise<EntryCreateRequest | null> {
    return new Promise((resolve => {
        entryFormReason.textContent = reason;
        if (input != null) {
            start.value = toLocalISOString(input.start);
            end.value = toLocalISOString(input.end);
            project.value = "";
            categories.innerHTML = [...input.categories].map(c => `<span>${c}</span>`).join();
            location.value = input.location ?? "";
        } else {
            start.value = "";
            end.value = "";
            project.value = "";
            categories.innerHTML = "";
            location.value = "";
        }
        entryDialog.returnValue = "close";
        entryDialog.addEventListener(
            "close",
            () => {
                if (entryDialog.returnValue === "close") {
                    resolve(null);
                }

                resolve({
                    start: new Date(start.value),
                    end: end.value != "" ? new Date(end.value) : null,
                    project: project.value != "" ? project.value : null,
                    categories: new Set<string>([...categories.querySelectorAll("span")].map(s => s.innerText)),
                    location: location.value != "" ? location.value : null,
                });
            },
            { once: true },
        );
        entryDialog.showModal();
    }));

    function toLocalISOString(date: Date | null): string {
        if (date == null) {
            return "";
        }
        const offsetMs = date.getTimezoneOffset() * 60 * 1000;
        const localTime = new Date(date.getTime() - offsetMs);
        return localTime.toISOString().substring(0, 16);
    }
}

export class EntryEditor {
    static openNew(): Promise<EntryCreateRequest | null> {
        return open("New", null);
    }

    static openEdit(input: Entry): Promise<EntryCreateRequest | null> {
        return open("Edit", input);
    }
}
