declare const settingsForm: HTMLFormElement;
const tokenKey = "token" as const;
declare const token: HTMLInputElement;

window.addEventListener(
    "load",
    () => {
        settingsForm.addEventListener("submit", () => localStorage.setItem(tokenKey, token.value));
        token.value = localStorage.getItem(tokenKey) ?? "";
    },
);

export class Settings {
    static get token(): string | null {
        return localStorage.getItem(tokenKey);
    }
}
