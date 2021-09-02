import path from "path";
import { Editor } from "./Editor";
import { Compiler } from "./Compiler";
import { TabManager } from "./TabManager";
import { Resource } from "./Resource";
const ipcRenderer = require("electron").ipcRenderer;
const dialog = require("electron").remote.dialog;

export let isCtrl = false;

async function main() {
	// TODO: You cannot use __dirname because it causes bugs when building
	// Save the app data to %App Data% instead
	await Compiler.init();

	setupConsole();

	// Get events from the main process
	ipcRenderer.on("cmd", function (evt, message) {
		console.log(message); // Returns: {'SAVED': 'File Saved'}
	});

	// Setup save event
	document.onkeyup = function (e) {
		if (e.keyCode == 17) isCtrl = false;
	};
	document.onkeydown = function (e) {
		if (e.keyCode == 17) isCtrl = true;
		if (isCtrl == true) {
			if (e.keyCode == 83) {
				TabManager.saveCurrent();
				return false;
			}

			if (e.keyCode == 79) {
				const files = dialog.showOpenDialogSync({
					properties: ["openFile", "multiSelections"],
				});

				for (const file of files || []) {
					console.log(file);
					TabManager.load(file);
				}
				return false;
			}
		}
	};

	// Load work space if it exists
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

window.onbeforeunload = (e) => {
	saveWorkspace();
};

main();

function setupConsole() {
	const toggler = document.getElementById("console_toggle");
	const container = document.getElementById("console_container");
	const cons = document.querySelector("#console pre");
	const cinInput = <HTMLInputElement>document.getElementById("stdcin");

	toggler!.onclick = () => {
		container?.classList.toggle("tranlate-out");
	};

	const newTab = document.getElementById("new");
	newTab!.onclick = () => {
		TabManager.open("untitled", new Editor("untitled"));
	};

	const play = document.getElementById("play");
	const stop = document.getElementById("stop");
	play!.onclick = async () => {
		container?.classList.remove("tranlate-out");

		const active = TabManager.getActive();

		cons!.innerHTML = `Running ${
			active.name
		} ${new Date().toLocaleString()}\n\n`;

		cinInput?.focus();

		Compiler.onProcessExit((code) => {
			stop!.style.display = "none";
			play!.style.display = "block";
			cinInput?.blur();
		});

		Compiler.runDynamicCode(active.editor.getContent(), (data) => {
			cons!.innerHTML += data.toString();
		});

		stop!.style.display = "block";
		play!.style.display = "none";
	};

	stop!.style.display = "none";
	stop!.onclick = () => {
		Compiler.killProcess();
	};

	cinInput!.onkeypress = (e) => {
		if (e.keyCode == 13) {
			Compiler.cin(cinInput?.value + "\n");
			cinInput!.value = "";
		}
	};
}

function saveWorkspace() {
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

export function pathToFilename(filepath: string) {
	return path.basename(filepath);
}
