const dialog = require("electron").remote.dialog;
import { Editor } from "./Editor";
import { Resource } from "./Resource";
import { TabManager } from "./TabManager";
import fs from "fs";
import path from "path";
import { Compiler } from "./Compiler";
const ipcRenderer = require("electron").ipcRenderer;

/**
 *
 */
interface ISettings {
	compilerPath: string;
	compilerDumpContent: boolean;
}

export default class Settings {
	private static readonly singleton = new Settings();
	private static readonly workspaceFilename = "shado.editor.workspace";
	private static readonly typesFilename = "editor.sscript.types";

	private settings: ISettings = {
		compilerPath: "",
		compilerDumpContent: false,
	};

	private constructor() {}

	public static init(): void {
		// Get the settings from the resouce file
		// Todo in the future use .shado files
		const filename = "settings";
		if (Resource.exists({ filename })) {
			try {
				this.singleton.settings = JSON.parse(
					Resource.loadResource({ filename })
				);
				return;
			} catch (e) {
				dialog.showErrorBox(
					"Error",
					"An error has occurred while parsing settings file!"
				);
			}
		}

		// Otherwise get the compiler filepath
		dialog.showMessageBoxSync({
			message:
				"Settings file not found, please locate your Shado Script compiler",
		});

		this.showSelectCompilerDialog();
	}

	public static loadWorkSpace() {
		let active = "";
		if (Resource.exists({ filename: this.workspaceFilename })) {
			const lines: string[] = Resource.loadResource({
				filename: "shado.editor.workspace",
			}).split("\n");

			for (const line of lines) {
				if (line == "") {
					continue;
				} else if (line.startsWith("ACTIVE")) {
					active = line.split("\t")[1].trim();
					continue;
				}

				const tokens = line.split("\t");
				try {
					TabManager.open(tokens[0], Editor.fromFile(tokens[1].trim()));
				} catch (e) {
					e = e as Error;
					dialog.showErrorBox("Failed to open file", e.message);
				}
			}
		} else {
			// Otherwise just add an empty editor
			TabManager.open("untitled", new Editor("untitled"));
		}
		TabManager.render();

		if (active) TabManager.setActive(TabManager.getTabByName(active));
	}

	public static saveWorkspace() {
		// Save the current workspace
		const filename = this.workspaceFilename;
		Resource.saveResource({ filename, content: "" });

		for (const tab of TabManager.allTabs()) {
			// If it is not untitled (meaning it is a physical file)
			if (tab.name != "untitled") {
				Resource.appendResource({
					filename,
					content: `${tab.name}\t${tab.editor.getFilepath()}\n`,
				});
			}
			// Otherwise have to save file
			else {
				const date = Date.now();

				// Save the content of the file
				Resource.saveResource({
					relativePath: "/temp",
					filename: `/untitled_${date}.sscript`,
					content: tab.editor.getContent(),
				});
				Resource.appendResource({
					filename,
					content: `${tab.name}_${date}\t${Resource.toFullPath(
						`/temp/untitled_${date}.sscript`
					)}\n`,
				});
			}
		}

		// Save the active tab
		Resource.appendResource({
			filename,
			content: `ACTIVE\t${TabManager.getActive().name}\n`,
		});
	}

	public static saveSettings() {
		Resource.saveResource({
			filename: "settings",
			content: JSON.stringify(this.singleton.settings),
		});
	}

	public static openSettingsWindow() {
		const remote = require("electron").remote;
		const BrowserWindow = remote.BrowserWindow;
		const win = new BrowserWindow({
			height: 600,
			width: 800,
			parent: BrowserWindow.getAllWindows()[0],
			webPreferences: {
				nodeIntegration: true,
				contextIsolation: false,
				enableRemoteModule: true,
				devTools: true, //is.development,
			},
		});
		win.removeMenu();
		win.loadURL(
			view("settings", {
				workspace: Resource.toFullPath(this.workspaceFilename),
				types: Resource.toFullPath(this.typesFilename),
				compiler: Compiler.compiler_path,
				core_lib: Compiler.getCoreLibPath(),
				dump_output: Compiler.dump_output + "",
			})
		);
		win.focusOnWebView();

		const lambda = (event: any, data: any) => {
			console.log(data.data);
			Compiler.dump_output = data.data;
			this.singleton.settings.compilerDumpContent = data.data;
		};

		ipcRenderer.on("send_data_to_parent_window", lambda);

		win.on("close", (e) => {
			ipcRenderer.off("send_data_to_parent_window", lambda);
		});
	}

	public static get(key: keyof ISettings) {
		return this.singleton.settings[key];
	}

	private static showSelectCompilerDialog() {
		const file = dialog.showOpenDialogSync({});
		if (file) {
			this.singleton.settings.compilerPath = file[0];
		} else {
			dialog.showErrorBox("Error", "The file is undefined");
			this.init();
		}
	}
}

function clearArray(array: any[]) {
	while (array.length) {
		array.pop();
	}
}

/**
 *
 * @param name view name without the .template.html extension
 */
function view(name: string, variables: Record<string, string> = {}) {
	let html = fs.readFileSync(
		path.join(__dirname, "../../" + name + ".template.html"),
		"utf8"
	);

	for (const variable in variables) {
		html = html.replaceAll(
			new RegExp(`%\\{${variable}}`, "g"),
			variables[variable]
		);
	}

	return "data:text/html;charset=UTF-8," + encodeURIComponent(html);
}
