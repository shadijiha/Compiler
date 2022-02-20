import path from "path";
import { Editor } from "./Editor";
import { Compiler } from "./Compiler";
import { TabManager } from "./TabManager";
import Settings from "./Settings";
import { getScrollbarWidth, hasScrollbarX } from "./util";
import { Resource } from "./Resource";
const ipcRenderer = require("electron").ipcRenderer;
const dialog = require("electron").remote.dialog;
import fs from "fs";
import { Runtime } from "./Runtime";

export let isCtrl = false;

async function main() {
	Settings.init();

	await Compiler.init();

	setupConsole();

	// Get events from the main process
	ipcRenderer.on("cmd", function (evt, message) {
		switch (message) {
			case "open":
				openFiles();
				break;
			case "save":
				saveFile();
				break;
			case "new":
				TabManager.open("untitled", new Editor("untitled"));
				break;
			case "settings":
				Settings.openSettingsWindow();
				break;
		}
	});

	// Setup save event
	document.onkeyup = function (e) {
		if (e.keyCode == 17) isCtrl = false;
	};
	document.onkeydown = function (e) {
		if (e.keyCode == 17) isCtrl = true;
	};

	// Attempt to load the keywords definitions file
	//Settings.loadTypesFile();

	// Watch if the types file has been modified after
	// The initial load
	/*fs.watchFile(Resource.toFullPath("editor.sscript.types"), (curr, prev) => {
		Settings.loadTypesFile();
		TabManager.formatAll();
	});*/

	// Load work space if it exists
	Settings.loadWorkSpace();
}

window.onbeforeunload = (e) => {
	Settings.saveWorkspace();
	Settings.saveSettings();
};

main();

function setupConsole() {
	const toggler = document.getElementById("console_toggle");
	const container = <HTMLDivElement>(
		document.getElementById("console_container")
	);
	const cons = document.querySelector("#console pre");
	const cinInput = <HTMLInputElement>document.getElementById("stdcin");

	let toggled = false;
	const animateConsoleToggler = () => {
		let width = container?.clientWidth;
		if (hasScrollbarX(container)) {
			width += getScrollbarWidth();
		}

		if (!toggled) {
			toggler!.style.right = width + "px";
		} else toggler!.style.right = "0px";
		toggled = !toggled;
	};

	toggler!.onclick = () => {
		container?.classList.toggle("tranlate-out");
		animateConsoleToggler();
	};

	const newTab = document.getElementById("new");
	newTab!.onclick = () => {
		TabManager.open("untitled", new Editor("untitled"));
	};

	const play = document.getElementById("play");
	const stop = document.getElementById("stop");
	play!.onclick = async () => {
		container?.classList.remove("tranlate-out");

		if (!toggled) animateConsoleToggler();

		const active = TabManager.getActive();

		cons!.innerHTML = `Running ${
			active.name
		} ${new Date().toLocaleString()}\n\n`;

		cinInput?.focus();

		Runtime.run({
			editor: active.editor,
			cinInput,
			consoleDOM: cons!,
			playBtn: play,
			stopBtn: stop,
		});

		stop!.style.display = "block";
		play!.style.display = "none";
	};

	stop!.style.display = "none";
}

export function pathToFilename(filepath: string) {
	return path.basename(filepath);
}

function openFiles() {
	const files = dialog.showOpenDialogSync({
		properties: ["openFile", "multiSelections"],
	});

	for (const file of files || []) {
		TabManager.load(file);
	}
	return false;
}

function saveFile() {
	TabManager.saveCurrent();
}
