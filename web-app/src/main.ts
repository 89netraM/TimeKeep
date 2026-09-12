import "./style.css";
import { EntryEditor } from "./entryEditor";
import "./settings";

declare const addButton: HTMLButtonElement;

window.addEventListener(
    "load",
    () => {
        addButton.addEventListener(
            "click",
            async () => {
                const entryCreateRequest = await EntryEditor.openNew()
                console.log({ entryCreateRequest });
            }
        );
    }
);
