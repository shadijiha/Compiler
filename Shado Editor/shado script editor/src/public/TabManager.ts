import path from "path";
import { Editor } from "./Editor";
import fs from "fs";
import { shell } from "electron";
import { pathToFilename } from ".";
import {
	displayMessageBox,
	getParentWindow,
	hideMessageBox,
	openContextMenu,
} from "./util";
import { Extensions } from "./Extensions";
const remote = require("electron").remote;
const dialog = remote.dialog;
const BrowserWindow = remote.BrowserWindow;

type Tab = { name: string; editor: Editor; extensionCheck: boolean };

export class TabManager {
	private static tabs: Tab[] = [];
	private static active: Tab;

	private constructor() {}

	public static load(filepath: string) {
		let temp: Tab | null = null;

		for (const e of this.tabs) {
			if (this.compareNames(e.name, filepath)) {
				temp = e;
			}

			e.editor.hide();
		}

		// If temp is null then the file hasn't been open yet
		if (!temp) {
			this.open(filepath, Editor.fromFile(filepath));
			return;
		}

		temp?.editor.show();
		if (temp) {
			this.setActive(temp);
		}
	}

	public static open(name: string, editor: Editor) {
		const temp = { name, editor, extensionCheck: true };
		this.tabs.push(temp);
		this.setActive(temp);
		this.load(name);

		editor.format();
		this.render();
	}

	public static close(name: string) {
		let newTabs: Tab[] = [];

		for (const [i, e] of this.tabs.entries()) {
			if (!this.compareNames(e.name, name)) {
				newTabs.push(e);
			}
		}

		const toDelete = this.tabs.find((e) => this.compareNames(e.name, name));
		toDelete?.editor.dispose();
		this.tabs = newTabs;

		// If it is the last tab open
		if (this.tabs.length == 0) {
			TabManager.open("untitled", new Editor("untitled"));
		}

		// IF it is the active tab
		if (this.compareNames(this.active.name, toDelete?.name ?? "")) {
			this.setActive(this.tabs[this.tabs.length - 1]);
		}

		this.render();
	}

	public static formatAll() {
		for (const tab of this.tabs) {
			tab.editor.format();
		}
	}

	public static async saveCurrent() {
		// Write the current editor content to the file path
		const filepath = this.active.editor.getFilepath();
		const content = this.active.editor.getContent();

		if (filepath != "untitled") {
			try {
				fs.writeFileSync(filepath, content);
			} catch (e) {
				const err = e as Error;
				dialog.showErrorBox("Error saving file", err.message);
			}
		} else {
			// Show save as dialog
			const data = await dialog.showSaveDialog(
				BrowserWindow.getAllWindows()[0],
				{
					title: "Save file as",
				}
			);

			try {
				if (!data.filePath) return;
				fs.writeFileSync(data.filePath, content);
				this.active.editor.setFilepath(data.filePath);
				this.active.name = data.filePath;
				this.render();
			} catch (e) {
				const err = e as Error;
				dialog.showErrorBox("Error saving file", err.message);
			}
		}
	}

	public static render(): void {
		const DOM = document.getElementById("tabs") as HTMLDivElement;
		DOM.innerHTML = "";

		for (const tab of this.tabs) {
			const divTab = document.createElement("div");
			const tabName = pathToFilename(tab.name);

			divTab.id = "__tab_" + tabName;
			divTab.title = tabName;
			divTab.innerHTML =
				tabName.length > 8 ? tabName.substring(0, 8) + "..." : tabName;

			if (this.active.name == tabName) {
				divTab.classList.add("selected");
			}

			divTab.onclick = () => {
				this.load(tab.name);
			};

			divTab.oncontextmenu = (e: any) => {
				openContextMenu(e, [
					{
						lable: "Open in explorer",
						onClick: () => {
							const p = path.join(tab.name);
							this.openExplorer(p);
						},
					},
				]);
			};

			const closeIcon = document.createElement("i");
			closeIcon.classList.add("fas");
			closeIcon.classList.add("close");
			closeIcon.classList.add("fa-times-circle");
			divTab.appendChild(closeIcon);

			closeIcon.onclick = (e) => {
				this.close(tab.name);
				e.stopPropagation();
			};

			DOM.appendChild(divTab);
		}
	}

	public static getActive(): Tab {
		return this.active;
	}

	public static async verifyTabExtension() {
		const tab = this.active;

		// Check if the langauge of the
		if (!tab || !tab.editor) return;

		const extension = tab.editor.getExtension();
		if (
			tab.extensionCheck &&
			!(await Extensions.isInstalled(extension)) &&
			(await Extensions.isOnClouud(extension))
		) {
			displayMessageBox(
				`You do not have extension for ${extension}. Would you like to install it? <i id="install_extension">Install</i>`,
				() => (tab.extensionCheck = false)
			);

			document.getElementById("install_extension")!.onclick = () => {
				try {
					Extensions.install(extension);
					displayMessageBox(
						"Successfully installed extensions for " +
							extension +
							"! Please reload the editor."
					);

					setTimeout(() => hideMessageBox(), 1000);
				} catch (err) {
					const e = (<Error>err).message;
					tab.extensionCheck = false;
					displayMessageBox(e);
				}
			};
		}
	}

	public static async setActive(tab: Tab | null) {
		hideMessageBox();

		if (!tab) return;

		this.active = tab;
		const divTab = document.getElementById("__tab_" + pathToFilename(tab.name));
		for (const tab of this.tabs) {
			tab.editor.hide();
			document
				.getElementById("__tab_" + pathToFilename(tab.name))
				?.classList.remove("selected");
		}

		divTab?.classList.add("selected");
		tab.editor.show();

		await this.verifyTabExtension();
	}

	public static allTabs() {
		return [...this.tabs];
	}

	public static getTabByName(name: string) {
		for (const tab of this.tabs) {
			if (tab.name == name) return tab;
		}
		return null;
	}

	private static compareNames(name1: string, name2: string) {
		return name1.replace(/[\/\\]/g, "") == name2.replace(/[\/\\]/g, "");
	}

	private static openExplorer(path: string) {
		shell.showItemInFolder(path);
	}
}
