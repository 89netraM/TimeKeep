import { toLocalISOString } from "./time";

declare const endDialog: HTMLDialogElement;
declare const endEnd: HTMLInputElement;

export class EndDialog {
    static end(): Promise<Date | null> {
        return new Promise(resolve => {
            endDialog.returnValue = "cancel";
            endEnd.value = toLocalISOString(new Date());
            endDialog.addEventListener(
                "close",
                () => {
                    if (endDialog.returnValue !== "end") {
                        resolve(null);
                    }

                    resolve(new Date(endEnd.value));
                },
                { once: true },
            );
            endDialog.inert = true;
            endDialog.showModal();
            endDialog.inert = false;
        });
    }
}
