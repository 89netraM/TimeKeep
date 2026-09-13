import type { Entry } from "./models";
import { formatDateTime } from "./time";

declare const entryTemplate: HTMLTemplateElement;

export class EntryElement extends HTMLElement {
    static readonly observedAttributes: ReadonlyArray<string> = ["entry"];

    #entry: Entry | null = null;
    public get entry(): Entry | null {
        return this.#entry;
    }
    public set entry(value: Entry | null) {
        this.#entry = value;
        if (this.#entry == null) {
            this.#from.innerText = "";
            this.#to.innerText = "";
            this.#location.innerText = "";
            this.#categories.innerHTML = "";
            this.#endButton.hidden = false;
        } else {
            this.#from.innerText = formatDateTime(this.#entry.start);
            if (this.#entry.end != null) {
                this.#to.innerText = formatDateTime(this.#entry.end);
                this.#endButton.hidden = true;
            } else {
                this.#to.innerText = "";
                this.#endButton.hidden = false;
            }
            this.#location.innerText = this.#entry.location ?? "";
            this.#categories.innerHTML = this.#entry.categories.map(c => `<span>${c}</span>`).join("");
        }
    }

    readonly #from: HTMLSpanElement;
    readonly #to: HTMLSpanElement;
    readonly #location: HTMLSpanElement;
    readonly #categories: HTMLDivElement;

    readonly #deleteButton: HTMLDivElement;
    readonly #editButton: HTMLDivElement;
    readonly #endButton: HTMLDivElement;

    constructor() {
        super();

        const shadowRoot = this.attachShadow({ mode: "closed" });
        shadowRoot.appendChild(document.importNode(entryTemplate.content, true));

        this.#from = shadowRoot.getElementById("from") as HTMLSpanElement;
        this.#to = shadowRoot.getElementById("to") as HTMLSpanElement;
        this.#location = shadowRoot.getElementById("location") as HTMLSpanElement;
        this.#categories = shadowRoot.getElementById("categories") as HTMLDivElement;

        this.#deleteButton = shadowRoot.getElementById("deleteButton") as HTMLDivElement;
        this.#editButton = shadowRoot.getElementById("editButton") as HTMLDivElement;
        this.#endButton = shadowRoot.getElementById("endButton") as HTMLDivElement;

        this.#deleteButton.addEventListener("click", this.#onButtonClick.bind(this, "delete"));
        this.#editButton.addEventListener("click", this.#onButtonClick.bind(this, "edit"));
        this.#endButton.addEventListener("click", this.#onButtonClick.bind(this, "end"));
    }

    attributeChangedCallback(name: string, _oldValue: any, newValue: any): void {
        if (name === "entry") {
            this.entry = newValue as Entry;
        }
    }

    #onButtonClick(type: string, e: PointerEvent): void {
        if (this.entry == null) {
            return;
        }

        this.dispatchEvent(new EntryButtonEvent(type, this.entry, e));
    }
}

customElements.define("entry-element", EntryElement);

export class EntryButtonEvent extends PointerEvent {
    public readonly entry: Entry;

    constructor(type: string, entry: Entry, original: PointerEvent) {
        super(type, { ...original, bubbles: true });
        this.entry = entry;
    }
}
