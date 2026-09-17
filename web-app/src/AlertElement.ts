declare const alerts: HTMLDivElement;
declare const alertTemplate: HTMLTemplateElement;

export class AlertElement extends HTMLElement {
    public static showAlert(content: string): void {
        const alert = document.createElement("alert-element");
        alert.innerText = content;
        alerts.appendChild(alert);
    }

    #removeController: AbortController = new AbortController();

    constructor() {
        super();

        const shadowRoot = this.attachShadow({ mode: "closed" });
        shadowRoot.appendChild(document.importNode(alertTemplate.content, true));
    }

    connectedCallback(): void {
        this.#removeController = new AbortController();
        this.addEventListener(
            "click",
            this.#remove.bind(this),
            {
                once: true,
                signal: this.#removeController.signal,
            },
        );
        const timeoutId = setTimeout(this.#remove.bind(this), 5000);
        this.#removeController.signal.addEventListener(
            "abort",
            () => clearTimeout(timeoutId),
            {
                once: true,
                signal: this.#removeController.signal,
            },
        );
    }

    #remove(): void {
        this.remove();
        this.#removeController.abort();
    }
}

customElements.define("alert-element", AlertElement);
