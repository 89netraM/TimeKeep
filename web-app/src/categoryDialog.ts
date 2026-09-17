import { createCategory } from "./api";

declare const newCategoryDialog: HTMLDialogElement;
declare const category: HTMLInputElement;
declare const allCategories: HTMLInputElement;

window.addEventListener(
    "load",
    () => {
        newCategoryDialog.addEventListener(
            "beforetoggle",
            e => {
                if (e.newState !== "open") {
                    return;
                }

                category.value = "";
                newCategoryDialog.returnValue = "close";
            },
        )
        newCategoryDialog.addEventListener(
            "close",
            async () => {
                if (newCategoryDialog.returnValue !== "save") {
                    return;
                }

                const categoryName = category.value;
                await createCategory({ category: categoryName });
                allCategories.innerHTML += `<option value="${categoryName}">${categoryName}</option>`;
            },
        );
    },
);
