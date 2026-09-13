import { EntryButtonEvent } from "./EntryElement";

declare global {
    interface GlobalEventHandlersEventMap {
        delete: EntryButtonEvent;
        edit: EntryButtonEvent;
        end: EntryButtonEvent;
    }
}
