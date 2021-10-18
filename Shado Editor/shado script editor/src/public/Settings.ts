const dialog = require("electron").remote.dialog;
import { Editor } from "./Editor";
import { Resource } from "./Resource";
import { TabManager } from "./TabManager";
import fs from "fs";
import path from "path";
import { Compiler } from "./Compiler";
import CodeFormatter from "./CodeFormatter";

/**
 *
 */
interface ISettings {
	compilerPath: string;
}

export default class Settings {
	private static readonly singleton = new Settings();
	private static readonly workspaceFilename = "shado.editor.workspace";
	private static readonly typesFilename = "editor.sscript.types";

	private settings: ISettings = {
		compilerPath: "",
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
		});
		win.removeMenu();
		win.loadURL(
			view("settings", {
				workspace: Resource.toFullPath(this.workspaceFilename),
				types: Resource.toFullPath(this.typesFilename),
				compiler: Compiler.compiler_path,
				core_lib: Compiler.getCoreLibPath(),
			})
		);
		win.focusOnWebView();
	}

	public static get(key: keyof ISettings) {
		return this.singleton.settings[key];
	}

	public static loadTypesFile() {
		const filename = this.typesFilename;
		let filecontent: string | null = null;

		// This file should be location in the %AppData%
		if (Resource.exists({ filename })) {
			filecontent = Resource.loadResource({ filename });
		}

		// If not then search in the compile Core.sscript path
		if (
			fs.existsSync(path.join(Compiler.getCoreLibPath(), filename)) &&
			!filecontent
		) {
			filecontent = fs.readFileSync(
				path.join(Compiler.getCoreLibPath(), filename),
				"utf-8"
			);
		}

		// Otherwise just default to the already defined keywords and constants
		if (!filecontent) return;

		// If the file exists then parse it
		// and clear the default CodeFormatter keywords
		clearArray(CodeFormatter.KEYWORDS);
		clearArray(CodeFormatter.MODIFIERS);
		clearArray(CodeFormatter.NATIVE_TYPES);
		clearArray(CodeFormatter.CONSTANTS);

		const lines = filecontent.split("\n");
		for (const line of lines) {
			const tokens = line.trim().split(/\s+/);
			switch (tokens[0]) {
				case "keyword":
					CodeFormatter.KEYWORDS.push(tokens[1]);
					break;
				case "modifier":
					CodeFormatter.MODIFIERS.push(tokens[1]);
					break;
				case "constant":
					CodeFormatter.CONSTANTS.push(tokens[1]);
					break;
				case "native_type":
					CodeFormatter.NATIVE_TYPES.push(tokens[1]);
					break;
			}
		}
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
