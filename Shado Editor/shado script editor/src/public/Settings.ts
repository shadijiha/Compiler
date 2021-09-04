const dialog = require("electron").remote.dialog;
import { Editor } from "./Editor";
import { Resource } from "./Resource";
import { TabManager } from "./TabManager";

/**
 *
 */
interface ISettings {
	compilerPath: string;
}

export default class Settings {
	private static readonly singleton = new Settings();

	private settings: ISettings = { compilerPath: "" };

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
		if (Resource.exists({ filename: "shado.editor.workspace" })) {
			const lines: string[] = Resource.loadResource({
				filename: "shado.editor.workspace",
			}).split("\n");

			for (const line of lines) {
				if (line == "") {
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
	}

	public static saveWorkspace() {
		// Save the current workspace
		const filename = "shado.editor.workspace";
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
	}

	public static saveSettings() {
		Resource.saveResource({
			filename: "settings",
			content: JSON.stringify(this.singleton.settings),
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
