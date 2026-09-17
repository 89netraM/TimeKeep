declare const settingsDialog: HTMLDialogElement;
const tokenKey = "token" as const;
declare const token: HTMLInputElement;

window.addEventListener(
    "load",
    () => {
        settingsDialog.addEventListener(
            "beforetoggle",
            e => {
                if (e.newState !== "open") {
                    return;
                }

                token.value = Settings.token ?? "";
                settingsDialog.returnValue = "close";
            },
        )
        settingsDialog.addEventListener(
            "close",
            () => {
                if (settingsDialog.returnValue !== "save") {
                    return;
                }

                localStorage.setItem(tokenKey, token.value);
                Settings.updateCallback?.();
            },
        );
    },
);

export class Settings {
    static get token(): string | null {
        return localStorage.getItem(tokenKey);
    }

    static updateCallback: (() => any) | null = null;
}
