export function formatDateTime(date: Date): string {
    let string = "";
    const now = new Date();
    if (date.toDateString() !== now.toDateString()) {
        string = `${date.getFullYear()}-${date.getMonth().toString().padStart(2, "0")}-${date.getDate().toString().padStart(2, "0")}`;
    }
    string += ` ${date.getHours().toString().padStart(2, "0")}:${date.getMinutes().toString().padStart(2, "0")}`;
    return string.trimStart();
}

export function toLocalISOString(date: Date | null): string {
    if (date == null) {
        return "";
    }
    const offsetMs = date.getTimezoneOffset() * 60 * 1000;
    const localTime = new Date(date.getTime() - offsetMs);
    return localTime.toISOString().substring(0, 16);
}
