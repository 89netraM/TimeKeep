declare const deleteDialog: HTMLDialogElement;

export class DeleteDialog {
    static delete(): Promise<boolean> {
        return new Promise(resolve => {
            deleteDialog.returnValue = "cancel";
            deleteDialog.addEventListener(
                "close",
                () => {
                    resolve(deleteDialog.returnValue === "delete");
                },
                { once: true },
            );
            deleteDialog.showModal();
        });
    }
}
