declare const newCategoryDialog: HTMLDialogElement;
declare const category: HTMLInputElement;

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
            () => {
                if (newCategoryDialog.returnValue !== "save") {
                    return;
                }

                console.log("TODO: Make RPC to create category");
            },
        );
    },
);
