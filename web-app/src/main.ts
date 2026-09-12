import "./style.css";
import "./categoryDialog";
import { EntryEditor } from "./entryEditor";
import "./projectDialog";
import "./settings";

declare const addButton: HTMLButtonElement;
declare const newButtons: HTMLDialogElement;

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
    }
);

async function createNewEntry(): Promise<void> {
    const entryCreateRequest = await EntryEditor.openNew()
    console.log({ entryCreateRequest });
}
