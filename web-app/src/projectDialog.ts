import { createProject } from "./api";

declare const newProjectDialog: HTMLDialogElement;
declare const project: HTMLSelectElement;
declare const newProject: HTMLSelectElement;
declare const newProjectCategories: HTMLSpanElement;
declare const newProjectCategoryInput: HTMLInputElement;
declare const newProjectAddCategoryButton: HTMLButtonElement;

window.addEventListener(
    "load",
    () => {
        newProjectCategoryInput.addEventListener(
            "keydown",
            e => {
                if (e.key !== "Enter") {
                    return;
                }

                e.preventDefault();
                addCategory();
            }
        );
        newProjectAddCategoryButton.addEventListener(
            "click",
            e => {
                e.preventDefault();
                addCategory();
            }
        );
        newProjectCategories.addEventListener(
            "click",
            e => {
                if (!(e.target instanceof HTMLElement) || e.target.parentElement != newProjectCategories) {
                    return;
                }

                e.target.remove();
            }
        );
        newProjectDialog.addEventListener(
            "beforetoggle",
            e => {
                if (e.newState !== "open") {
                    return;
                }

                newProject.value = "";
                newProjectCategories.innerHTML = "";
                newProjectDialog.returnValue = "close";
            },
        )
        newProjectDialog.addEventListener(
            "close",
            async () => {
                if (newProjectDialog.returnValue !== "save") {
                    return;
                }

                const projectName = newProject.value;
                await createProject({
                    project: projectName,
                    categories: [...newProjectCategories.querySelectorAll("span")].map(s => s.innerText)
                });
                project.innerHTML += `<option value="${projectName}">${projectName}</option>`;
            },
        );
    },
);

function addCategory() {
    if (newProjectCategoryInput.value.length <= 0) {
        return;
    }

    newProjectCategories.innerHTML += `<span>${newProjectCategoryInput.value}</span>`;
    newProjectCategoryInput.value = "";
}
